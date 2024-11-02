/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalProcessor"/> responsible for processing command batches in a terminal environment.
    /// </summary>
    public sealed class TerminalProcessor : ITerminalProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalProcessor"/> class.
        /// </summary>
        /// <param name="commandRouter">The command router to process commands.</param>
        /// <param name="terminalExceptionHandler">The handler for exceptions thrown during command processing.</param>
        /// <param name="terminalOptions">Configuration options for the terminal.</param>
        /// <param name="textHandler">The terminal text handler.</param>
        /// <param name="logger">Logger for logging operations within the queue.</param>
        public TerminalProcessor(
            ICommandRouter commandRouter,
            ITerminalExceptionHandler terminalExceptionHandler,
            IOptions<TerminalOptions> terminalOptions,
            ITerminalTextHandler textHandler,
            ILogger<TerminalProcessor> logger)
        {
            this.commandRouter = commandRouter;
            this.terminalExceptionHandler = terminalExceptionHandler;
            this.terminalOptions = terminalOptions;
            this.textHandler = textHandler;
            this.logger = logger;

            concurrentRequestQueue = new ConcurrentQueue<TerminalProcessorRequest>();
            concurrentResponseQueue = new ConcurrentDictionary<string, TerminalProcessorResponse>();
            requestProcessing = Task.CompletedTask;
            responseProcessing = Task.CompletedTask;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="TerminalProcessor"/> is currently processing requests.
        /// </summary>
        public bool IsProcessing { get; private set; }

        /// <summary>
        /// Gets the collection of unprocessed request at the time of query. It is a snapshot of the queue at the time
        /// of query and may not be accurate by the time the caller processes the collection.
        /// </summary>
        /// <remarks>
        /// THIS METHOD IS PART OF INTERNAL INFRASTRUCTURE AND IS NOT INTENDED TO BE USED BY APPLICATION CODE.
        /// </remarks>
        public IReadOnlyCollection<TerminalProcessorRequest> UnprocessedRequests => concurrentRequestQueue.ToList().AsReadOnly();

        /// <summary>
        /// Asynchronously adds a terminal request for processing from a string.
        /// </summary>
        public Task AddAsync(string batch, string? senderEndpoint, string? senderId)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(batch))
                {
                    throw new TerminalException(TerminalErrors.InvalidRequest, "The batch cannot be empty.");
                }

                if (batch.Length > terminalOptions.Value.Router.RemoteBatchMaxLength)
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "Batch length exceeds configured maximum. max_length={0}", terminalOptions.Value.Router.RemoteBatchMaxLength);
                }

                bool isBatch = false;
                if (terminalOptions.Value.Router.EnableRemoteDelimiters.GetValueOrDefault())
                {
                    if (!batch.EndsWith(terminalOptions.Value.Router.RemoteBatchDelimiter, textHandler.Comparison))
                    {
                        throw new TerminalException(TerminalErrors.InvalidConfiguration, "Batch processing is enabled but message does not ends with a batch delimiter.");
                    }

                    isBatch = true;
                }
                else
                {
                    if (batch.EndsWith(terminalOptions.Value.Router.RemoteBatchDelimiter, textHandler.Comparison))
                    {
                        throw new TerminalException(TerminalErrors.InvalidConfiguration, "Batch processing is not enabled but message ends with a batch delimiter.");
                    }
                }

                if (isBatch)
                {
                    EnqueueBatch(batch, senderEndpoint, senderId);
                }
                else
                {
                    EnqueueCommand(batch, senderEndpoint, senderId);
                }
            });
        }

        /// <summary>
        /// Starts background processing.
        /// </summary>
        /// <param name="terminalRouterContext">The terminal router context.</param>
        public void StartProcessing(TerminalRouterContext terminalRouterContext)
        {
            requestProcessing = StartRequestProcessingAsync(terminalRouterContext);
            responseProcessing = StartResponseProcessingAsync(terminalRouterContext);
            IsProcessing = true;
        }

        /// <inheritdoc/>
        public async Task<bool> StopProcessingAsync(int timeout)
        {
            if (!IsProcessing)
            {
                return false;
            }

            // Create a task that completes when both requestProcessing and responseProcessing are done
            var combinedTask = Task.WhenAll(requestProcessing, responseProcessing);
            if (combinedTask.Status != TaskStatus.RanToCompletion)
            {
                var task = await Task.WhenAny(combinedTask, Task.Delay(timeout));
                if (task != combinedTask)
                {
                    IsProcessing = true;
                    return true;
                }
            }

            IsProcessing = false;
            return false;
        }

        /// <summary>
        /// Starts a <see cref="Task.Delay(int, CancellationToken)"/> indefinitely until canceled.
        /// </summary>
        /// <param name="terminalRouterContext">The terminal router context.</param>
        public async Task WaitAsync(TerminalRouterContext terminalRouterContext)
        {
            try
            {
                await Task.Delay(Timeout.Infinite, terminalRouterContext.StartContext.TerminalCancellationToken);
            }
            catch (TaskCanceledException)
            {
                logger.LogDebug("Indefinite processing canceled.");
            }
        }

        /// <summary>
        /// Processes and enqueues a batch of messages from a string based on delimiters.
        /// </summary>
        private void EnqueueBatch(string batch, string? senderEndpoint, string? senderId)
        {
            string[] messages = batch.Split(new[] { terminalOptions.Value.Router.RemoteCommandDelimiter, terminalOptions.Value.Router.RemoteBatchDelimiter }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string msd in messages)
            {
                EnqueueCommand(msd, senderEndpoint, senderId);
            }
        }

        /// <summary>
        /// Enqueues a parsed message for processing.
        /// </summary>
        private void EnqueueCommand(string commmand, string? senderEndpoint, string? senderId)
        {
            var request = new TerminalProcessorRequest(Guid.NewGuid().ToString(), commmand, senderEndpoint, senderId);
            concurrentRequestQueue.Enqueue(request);
        }

        private async Task<CommandRoute?> ProcessRawCommandAsync(TerminalProcessorRequest item, TerminalRouterContext terminalRouterContext)
        {
            if (item.SenderId != null)
            {
                logger.LogDebug("Routing the command. raw={0} sender={1}", item.CommandString, item.SenderId);
            }
            else
            {
                logger.LogDebug("Routing the command. raw={0}", item.CommandString);
            }

            Dictionary<string, object> properties = new()
            {
                { TerminalIdentifiers.SenderEndpointToken, item.SenderEndpoint ?? "$unknown$" }
            };
            if (item.SenderId != null)
            {
                properties.Add(TerminalIdentifiers.SenderIdToken, item.SenderId);
            }

            var context = new CommandRouterContext(item.CommandString, terminalRouterContext, properties);
            var routeTask = commandRouter.RouteCommandAsync(context);

            if (await Task.WhenAny(routeTask, Task.Delay(terminalOptions.Value.Router.Timeout, terminalRouterContext.StartContext.TerminalCancellationToken)) == routeTask)
            {
                await routeTask;
                return context.Route;
            }
            else
            {
                throw new TimeoutException($"The command router timed out in {terminalOptions.Value.Router.Timeout} milliseconds.");
            }
        }

        private Task StartRequestProcessingAsync(TerminalRouterContext terminalRouterContext)
        {
            return Task.Run(async () =>
            {
                while (!terminalRouterContext.StartContext.TerminalCancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        if (concurrentRequestQueue.TryDequeue(out TerminalProcessorRequest? item))
                        {
                            await ProcessRawCommandAsync(item, terminalRouterContext);
                        }
                        else
                        {
                            await Task.Delay(100); // Prevent CPU hogging
                        }
                    }
                    catch (Exception ex)
                    {
                        await terminalExceptionHandler.HandleExceptionAsync(new TerminalExceptionHandlerContext(ex, null));
                    }
                }

                logger.LogDebug("Command queue processing canceled.");
            });
        }

        private Task StartResponseProcessingAsync(TerminalRouterContext terminalRouterContext) => Task.CompletedTask;

        private readonly ICommandRouter commandRouter;
        private readonly ConcurrentQueue<TerminalProcessorRequest> concurrentRequestQueue;
        private readonly ConcurrentDictionary<string, TerminalProcessorResponse> concurrentResponseQueue;
        private readonly ILogger logger;
        private readonly ITerminalExceptionHandler terminalExceptionHandler;
        private readonly IOptions<TerminalOptions> terminalOptions;
        private readonly ITerminalTextHandler textHandler;
        private Task requestProcessing;
        private Task responseProcessing;
    }
}
