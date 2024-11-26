/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;

using OneImlx.Terminal.Extensions;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The default implementation of <see cref="ITerminalProcessor"/> for processing inputs in a terminal environment.
    /// </summary>
    /// <remarks>
    /// The <see cref="TerminalProcessor"/> manages a queue of <see cref="TerminalInput"/> that are processed
    /// asynchronously in the background. It routes individual requests to the <see cref="ICommandRouter"/> for
    /// execution. The processor supports handling both single requests and batches of requests, as well as partial
    /// streams sent by clients.
    /// </remarks>
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

            processedRequests = [];
            unprocessedRequests = [];
            requestProcessing = Task.CompletedTask;
            responseProcessing = Task.CompletedTask;
            requestSignal = new SemaphoreSlim(0);
            responseSignal = new SemaphoreSlim(0);

            streamingRequests = new ConcurrentDictionary<string, Queue<byte>>();
        }

        /// <inheritdoc/>
        public bool IsBackground { get; private set; }

        /// <inheritdoc/>
        public bool IsProcessing { get; private set; }

        /// <inheritdoc/>
        public IReadOnlyCollection<TerminalInput> UnprocessedInputs
        {
            get
            {
                // Return all the requests from the unprocessed response queue.
                return unprocessedRequests.Select(r => r.Input).ToArray();
            }
        }

        /// <inheritdoc/>
        public Task AddAsync(TerminalInput input, string? senderId, string? senderEndpoint)
        {
            if (input == null)
            {
                throw new TerminalException(TerminalErrors.InvalidRequest, "The input cannot be null.");
            }

            if (!IsProcessing)
            {
                throw new TerminalException(TerminalErrors.InvalidRequest, "The terminal processor is not running.");
            }

            if (!IsBackground)
            {
                throw new TerminalException(TerminalErrors.InvalidRequest, "The terminal processor is not running a background queue.");
            }

            // Create a response object that will hold requests and results
            TerminalOutput response = new(input, new object?[input.Count], senderId, senderEndpoint);
            unprocessedRequests.Enqueue(response);
            requestSignal.Release();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            requestSignal.Dispose();
            responseSignal.Dispose();
            return new ValueTask(Task.CompletedTask);
        }

        /// <inheritdoc/>
        public async Task<TerminalOutput> ExecuteAsync(TerminalInput input, string? senderId, string? senderEndpoint)
        {
            if (input == null)
            {
                throw new TerminalException(TerminalErrors.InvalidRequest, "The input cannot be null.");
            }

            if (!IsProcessing || terminalRouterContext == null)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The terminal processor is not running.");
            }

            TerminalOutput response = new(input, new object?[input.Count], senderId, senderEndpoint);
            await RouteRequestsAsync(response, terminalRouterContext);
            return response;
        }

        /// <inheritdoc/>
        public void StartProcessing(TerminalRouterContext terminalRouterContext, bool background, Func<TerminalOutput, Task>? responseHandler = null)
        {
            // IMPORTANT: We don't await so both request and response processing happens in the background.
            requestProcessing = StartRequestProcessingAsync(terminalRouterContext, background);
            responseProcessing = StartResponseProcessingAsync(terminalRouterContext);

            IsProcessing = true;
            IsBackground = background;

            if (responseHandler != null)
            {
                RegisterResponseHandler(responseHandler);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> StopProcessingAsync(int timeout)
        {
            if (!IsProcessing)
            {
                throw new TerminalException(TerminalErrors.InvalidRequest, "The terminal processor is not running.");
            }

            var combinedTask = Task.WhenAll(requestProcessing, responseProcessing);
            if (combinedTask.Status != TaskStatus.RanToCompletion)
            {
                var task = await Task.WhenAny(combinedTask, Task.Delay(timeout));
                if (task != combinedTask)
                {
                    return true;
                }
            }

            IsProcessing = false;
            IsBackground = false;
            return false;
        }

        /// <inheritdoc/>
        public async Task StreamAsync(byte[] bytes, int bytesLength, string senderId, string? senderEndpoint)
        {
            if (string.IsNullOrWhiteSpace(senderId))
            {
                throw new TerminalException(TerminalErrors.InvalidRequest, "The sender ID cannot be null or empty for streaming.");
            }

            // Ensure a MemoryStream exists for the sender
            if (!streamingRequests.TryGetValue(senderId, out Queue<byte>? previousStream))
            {
                previousStream = new Queue<byte>();
                streamingRequests[senderId] = previousStream;
            }

            // Make sure the previous unprocessed stream is processed first
            for (int idx = 0; idx < bytesLength; ++idx)
            {
                previousStream.Enqueue(bytes[idx]);
            }

            // Split the input stream into batches using the stream delimiter
            byte[][] rawInputs = previousStream.ToArray().Split(terminalOptions.Value.Router.StreamDelimiter, ignoreEmpty: true, out bool endsWithDelimiter);
            previousStream.Clear();

            // Check if the last batch ends with the delimiter
            int lengthToProcess = rawInputs.Length;
            if (!endsWithDelimiter)
            {
                lengthToProcess--;
                byte[] last = rawInputs[lengthToProcess];
                for (int jdx = 0; jdx < last.Length; ++jdx)
                {
                    previousStream.Enqueue(last[jdx]);
                }
            }

            // We are good to route requests.
            for (int idx = 0; idx < lengthToProcess; ++idx)
            {
                byte[] rawInput = rawInputs[idx];

                TerminalInput? input = JsonSerializer.Deserialize<TerminalInput>(rawInputs[idx]);
                if (input == null)
                {
                    throw new TerminalException(TerminalErrors.InvalidRequest, "The input bytes cannot be deserialized to terminal input.");
                }
                await AddAsync(input, senderId, senderEndpoint);
            }
        }

        /// <inheritdoc/>
        public async Task WaitUntilCanceledAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                logger.LogDebug("Terminal processor indefinite wait canceled.");
            }
        }

        private void RegisterResponseHandler(Func<TerminalOutput, Task> handler)
        {
            if (!terminalOptions.Value.Router.EnableResponses)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The response handling is not enabled.");
            }

            this.handler = handler ?? throw new TerminalException(TerminalErrors.InvalidRequest, "The response handler cannot be null.");
        }

        private async Task RouteRequestsAsync(TerminalOutput terminalOutput, TerminalRouterContext terminalRouterContext)
        {
            for (int idx = 0; idx < terminalOutput.Input.Count; ++idx)
            {
                TerminalRequest request = terminalOutput.Input[idx];

                string senderEndpoint = terminalOutput.SenderEndpoint ?? "$unknown$";
                string senderId = terminalOutput.SenderId ?? "$unknown$";
                Dictionary<string, object> properties = new()
            {
                { TerminalIdentifiers.SenderEndpointToken, senderEndpoint },
                { TerminalIdentifiers.SenderIdToken, senderId }
            };

                if (request.Raw.Length > terminalOptions.Value.Router.MaxLength)
                {
                    throw new TerminalException(TerminalErrors.InvalidRequest, "The command length exceeds the maximum allowed. max={0}", terminalOptions.Value.Router.MaxLength);
                }

                logger.LogDebug("Routing the command. raw={0} sender={1}", request.Raw, senderId);
                var context = new CommandRouterContext(request, terminalRouterContext, properties);
                var routeTask = commandRouter.RouteCommandAsync(context);

                if (await Task.WhenAny(routeTask, Task.Delay(terminalOptions.Value.Router.Timeout, terminalRouterContext.StartContext.TerminalCancellationToken)) == routeTask)
                {
                    var result = await routeTask;
                    object? value = result.HandlerResult.RunnerResult.HasValue ? result.HandlerResult.RunnerResult.Value : null;
                    terminalOutput.Results[idx] = value;
                }
                else
                {
                    throw new TimeoutException($"The command router timed out in {terminalOptions.Value.Router.Timeout} milliseconds.");
                }
            }
        }

        private async Task StartRequestProcessingAsync(TerminalRouterContext terminalRouterContext, bool background)
        {
            // If we are not running a background process, return a completed task.
            this.terminalRouterContext = terminalRouterContext;
            if (!background)
            {
                return;
            }

            bool responseEnabled = terminalOptions.Value.Router.EnableResponses;

            while (true)
            {
                try
                {
                    // Wait until there is a signal or the cancellation. The requestSignal is used to signal that there
                    // is a new item in the queue, at the same time we don't hog the CPU in the outer while loop.
                    await requestSignal.WaitAsync(terminalRouterContext.StartContext.TerminalCancellationToken);
                    if (!unprocessedRequests.IsEmpty)
                    {
                        // Process the request and dequeue the response
                        unprocessedRequests.TryDequeue(out TerminalOutput? response);
                        if (response != null)
                        {
                            // Request is processed and results are populated in the response, not push it to processed requests.
                            await RouteRequestsAsync(response, terminalRouterContext);
                            processedRequests.Push(response);
                            responseSignal.Release();
                        }
                        else
                        {
                            logger.LogWarning("Failed to dequeue an unprocessed request.");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    logger.LogDebug("Processing canceled due to cancellation token.");
                    break;
                }
                catch (Exception ex)
                {
                    await terminalExceptionHandler.HandleExceptionAsync(new TerminalExceptionHandlerContext(ex, null));
                }
            }
        }

        private async Task StartResponseProcessingAsync(TerminalRouterContext terminalRouterContext)
        {
            if (!terminalOptions.Value.Router.EnableResponses)
            {
                return;
            }

            // The infinite while(true) enable a continuous processing of the command queue until canceled.
            while (true)
            {
                try
                {
                    // Wait until there is a signal or the cancellation is requested. The responseSignal is used to
                    // signal that there is a new item in the queue, at the same time we don't hog the CPU in the outer
                    // while loop.
                    await responseSignal.WaitAsync(terminalRouterContext.StartContext.TerminalCancellationToken);

                    // Invoke the handler for the response asynchronously.
                    if (handler != null)
                    {
                        Task invokeResponse = handler.Invoke(processedRequests.Pop());
                    }
                }
                catch (OperationCanceledException oex)
                {
                    // If canceled, break the while loop and exit the processing.
                    await terminalExceptionHandler.HandleExceptionAsync(new TerminalExceptionHandlerContext(oex, null));
                    break;
                }
                catch (Exception ex)
                {
                    await terminalExceptionHandler.HandleExceptionAsync(new TerminalExceptionHandlerContext(ex, null));
                }
            }
        }

        private readonly ICommandRouter commandRouter;
        private readonly ILogger logger;
        private readonly Stack<TerminalOutput> processedRequests;
        private readonly SemaphoreSlim requestSignal;
        private readonly SemaphoreSlim responseSignal;
        private readonly ConcurrentDictionary<string, Queue<byte>> streamingRequests;
        private readonly ITerminalExceptionHandler terminalExceptionHandler;
        private readonly IOptions<TerminalOptions> terminalOptions;
        private readonly ITerminalTextHandler textHandler;
        private readonly ConcurrentQueue<TerminalOutput> unprocessedRequests;
        private Func<TerminalOutput, Task>? handler;
        private Task requestProcessing;
        private Task responseProcessing;
        private TerminalRouterContext? terminalRouterContext;
    }
}
