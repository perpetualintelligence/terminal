/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// Manages the queue of terminal commands and processes them asynchronously.
    /// </summary>
    public sealed class TerminalCommandQueue
    {
        private readonly ICommandRouter commandRouter;
        private readonly ITerminalExceptionHandler terminalExceptionHandler;
        private readonly TerminalOptions terminalOptions;
        private readonly TerminalRouterContext terminalRouterContext;
        private readonly ILogger logger;
        private ConcurrentQueue<TerminalCommandQueueItem> concurrentQueue;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="commandRouter">The command router to process commands.</param>
        /// <param name="terminalExceptionHandler">The handler for exceptions thrown during command processing.</param>
        /// <param name="terminalOptions">Configuration options for the terminal.</param>
        /// <param name="terminalRouterContext">Context information for the terminal router.</param>
        /// <param name="logger">Logger for logging operations within the queue.</param>
        public TerminalCommandQueue(
            ICommandRouter commandRouter,
            ITerminalExceptionHandler terminalExceptionHandler,
            TerminalOptions terminalOptions,
            TerminalRouterContext terminalRouterContext,
            ILogger logger)
        {
            this.commandRouter = commandRouter;
            this.terminalExceptionHandler = terminalExceptionHandler;
            this.terminalOptions = terminalOptions;
            this.terminalRouterContext = terminalRouterContext;
            this.logger = logger;

            concurrentQueue = new ConcurrentQueue<TerminalCommandQueueItem>();
        }

        /// <summary>
        /// Enqueues a command for processing in the queue.
        /// </summary>
        /// <param name="command">The command string to enqueue.</param>
        /// <param name="sender">The endpoint of the sender who issued the command.</param>
        public void Enqueue(string command, IPEndPoint sender)
        {
            string[] splitCmdString = command.Split(new[] { terminalOptions.Router.MessageDelimiter }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string raw in splitCmdString)
            {
                if (raw.Length > terminalOptions.Router.MaxMessageLength)
                {
                    throw new TerminalException(TerminalErrors.InvalidRequest, "The command string length is over the configured limit. max_length={0}", terminalOptions.Router.MaxMessageLength);
                }

                logger.LogDebug("Enqueueing command. sender={0} command={1}", sender, raw);
                TerminalCommandQueueItem item = new(Guid.NewGuid().ToString(), raw, sender);
                concurrentQueue.Enqueue(item);
            }
        }

        /// <summary>
        /// Starts processing the command queue in the background.
        /// </summary>
        public Task StartCommandProcessing()
        {
            // Start processing the command queue immediately in the background. The code starts the background task for processing commands immediately
            // and does not wait for it to complete. We do not await it in this context. It effectively runs in the background, processing commands as they are enqueued.
            return Task.Run(ProcessCommandQueueAsync, terminalRouterContext.StartContext.TerminalCancellationToken);
        }

        private async Task ProcessCommandQueueAsync()
        {
            CancellationToken cancellationToken = terminalRouterContext.StartContext.TerminalCancellationToken;
            CommandRoute? commandRoute = null;
            while (!cancellationToken.IsCancellationRequested)
            {
                if (concurrentQueue.TryDequeue(out TerminalCommandQueueItem item))
                {
                    try
                    {
                        commandRoute = await ProcessRawCommandAsync(item.Command);
                    }
                    catch (Exception ex)
                    {
                        await terminalExceptionHandler.HandleExceptionAsync(new TerminalExceptionHandlerContext(ex, commandRoute));
                    }
                }
                else
                {
                    await Task.Yield();
                }
            }

            logger.LogDebug("Command queue processing cancelled.");
        }

        private async Task<CommandRoute?> ProcessRawCommandAsync(string raw)
        {
            logger.LogDebug("Routing the command. raw={0}", raw);

            // Route the command request to router. Wait for the router or the timeout.
            CommandRouterContext context = new(raw, terminalRouterContext);
            CommandRoute? commandRoute = context.Route;
            Task<CommandRouterResult> routeTask = commandRouter.RouteCommandAsync(context);
            if (await Task.WhenAny(routeTask, Task.Delay(terminalOptions.Router.Timeout, terminalRouterContext.StartContext.TerminalCancellationToken)) == routeTask)
            {
                // Task completed within timeout.
                // Consider that the task may have faulted or been canceled.
                // We re-await the task so that any exceptions/cancellation is re-thrown.
                await routeTask;
            }
            else
            {
                throw new TimeoutException($"The command router timed out in {terminalOptions.Router.Timeout} milliseconds.");
            }

            return commandRoute;
        }
    }
}