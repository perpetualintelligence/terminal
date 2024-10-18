/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// Manages the queue of terminal commands and processes them asynchronously.
    /// </summary>
    public sealed class TerminalRemoteQueue
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="commandRouter">The command router to process commands.</param>
        /// <param name="terminalExceptionHandler">The handler for exceptions thrown during command processing.</param>
        /// <param name="terminalOptions">Configuration options for the terminal.</param>
        /// <param name="terminalRouterContext">Context information for the terminal router.</param>
        /// <param name="logger">Logger for logging operations within the queue.</param>
        public TerminalRemoteQueue(
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

            concurrentRequestQueue = new ConcurrentQueue<TerminalRemoteQueueRequest>();
            requestProcessing = Task.CompletedTask;
            responseProcessing = Task.CompletedTask;
        }

        /// <summary>
        /// The current count of the message queue.
        /// </summary>
        public int RequestCount => concurrentRequestQueue.Count;

        /// <summary>
        /// Enqueues commands in the message for processing in the queue.
        /// </summary>
        /// <param name="message">The command string to enqueue.</param>
        /// <param name="senderEndpoint">The sender endpoint.</param>
        /// <param name="senderId">The sender id.</param>
        /// <returns>The list of enqueued items.</returns>
        public IEnumerable<TerminalRemoteQueueRequest> Enqueue(string message, string? senderEndpoint, string? senderId)
        {
            // Check message limit
            if (message.Length > terminalOptions.Router.RemoteMessageMaxLength)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The message length is more than configured limit. max_length={0}", terminalOptions.Router.RemoteMessageMaxLength);
            }

            // Ensure remote message delimiter is correctly configured.
            bool isPartial = false;
            if (terminalOptions.Router.EnableRemoteDelimiters.GetValueOrDefault())
            {
                if (!message.EndsWith(terminalOptions.Router.RemoteMessageDelimiter))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The remote message delimiter is enabled but message does not ends with a delimiter.");
                }

                isPartial = true;
            }
            else
            {
                if (message.EndsWith(terminalOptions.Router.RemoteMessageDelimiter))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The remote message delimiter is not enabled but message ends with a delimiter.");
                }
            }

            // If the command is a single command, it is enqueued directly. Otherwise, it is split into multiple
            // commands and enqueued.
            List<TerminalRemoteQueueRequest> items = [];
            if (isPartial)
            {
                logger.LogDebug("Received delimited message. sender_endpoint={0} sender_id={1} message={2}", senderEndpoint, senderId, message);

                // Now start processing command and move the partial command to the partial command dictionary.
                string[] splitCommands = message.Split(new[] { terminalOptions.Router.RemoteCommandDelimiter, terminalOptions.Router.RemoteMessageDelimiter }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string cmd in splitCommands)
                {
                    TerminalRemoteQueueRequest item = new(Guid.NewGuid().ToString(), cmd, senderEndpoint, senderId);
                    concurrentRequestQueue.Enqueue(item);
                    items.Add(item);
                }
            }
            else
            {
                TerminalRemoteQueueRequest item = new(Guid.NewGuid().ToString(), message, senderEndpoint, senderId);
                concurrentRequestQueue.Enqueue(item);
                items.Add(item);
            }

            return items;
        }

        /// <summary>
        /// Starts processing the command request and response queue in the background.
        /// </summary>
        public Task StartBackgroundCommandProcessingAsync()
        {
            // Start processing the command request queue immediately in the background. The code starts the background
            // task for processing commands immediately and does not wait for it to complete. We do not await it in this
            // context. It effectively runs in the background, processing commands as they are enqueued.
            requestProcessing = StartRequestProcessingAsync();

            // Start processing the command response queue immediately in the background. The code starts the background
            // task for processing commands immediately and does not wait for it to complete. We do not await it in this
            // context. It effectively runs in the background, processing commands as they are enqueued.
            responseProcessing = StartResponseProcessingAsync();

            return Task.WhenAll(requestProcessing, responseProcessing);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public async Task StopBackgroundCommandProcessingAsync()
        {
            // Stop processing the command request queue.
            await requestProcessing;

            // Stop processing the command response queue.
            await responseProcessing;
        }

        private async Task<CommandRoute?> ProcessRawCommandAsync(TerminalRemoteQueueRequest item)
        {
            if (item.SenderId != null)
            {
                logger.LogDebug("Routing the command. raw={0} sender={1}", item.CommandString, item.SenderId);
            }
            else
            {
                logger.LogDebug("Routing the command. raw={0}", item.CommandString);
            }

            // Setup the remote meta-data for the command.
            Dictionary<string, object> properties = [];
            properties.Add(TerminalIdentifiers.SenderEndpointToken, item.SenderEndpoint ?? "$unknown$");
            if (item.SenderId != null)
            {
                properties.Add(TerminalIdentifiers.SenderIdToken, item.SenderId);
            }

            // Route the command request to router. Wait for the router or the timeout.
            CommandRouterContext context = new(item.CommandString, terminalRouterContext, properties);
            CommandRoute? commandRoute = context.Route;
            Task<CommandRouterResult> routeTask = commandRouter.RouteCommandAsync(context);
            if (await Task.WhenAny(routeTask, Task.Delay(terminalOptions.Router.Timeout, terminalRouterContext.StartContext.TerminalCancellationToken)) == routeTask)
            {
                // Task completed within timeout. Consider that the task may have faulted or been canceled. We re-await
                // the task so that any exceptions/cancellation is re-thrown.
                await routeTask;
            }
            else
            {
                throw new TimeoutException($"The command router timed out in {terminalOptions.Router.Timeout} milliseconds.");
            }

            return commandRoute;
        }

        private Task StartRequestProcessingAsync()
        {
            return Task.Run(async () =>
            {
                CancellationToken cancellationToken = terminalRouterContext.StartContext.TerminalCancellationToken;
                CommandRoute? commandRoute = null;
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (concurrentRequestQueue.TryDequeue(out TerminalRemoteQueueRequest? item))
                    {
                        try
                        {
                            commandRoute = await ProcessRawCommandAsync(item);
                        }
                        catch (Exception ex)
                        {
                            await terminalExceptionHandler.HandleExceptionAsync(new TerminalExceptionHandlerContext(ex, commandRoute));
                        }
                    }
                    else
                    {
                        // Avoid CPU hogging by waiting for a short period before checking the queue again.
                        if (concurrentRequestQueue.IsEmpty)
                        {
                            await Task.Delay(100);
                        }
                    }
                }

                logger.LogDebug("Command queue processing canceled.");
            });
        }

        private Task StartResponseProcessingAsync()
        {
            return Task.CompletedTask;
        }

        private readonly ICommandRouter commandRouter;
        private readonly ConcurrentQueue<TerminalRemoteQueueRequest> concurrentRequestQueue;
        private readonly ILogger logger;
        private readonly ITerminalExceptionHandler terminalExceptionHandler;
        private readonly TerminalOptions terminalOptions;
        private readonly TerminalRouterContext terminalRouterContext;
        private Task requestProcessing;
        private Task responseProcessing;
    }
}
