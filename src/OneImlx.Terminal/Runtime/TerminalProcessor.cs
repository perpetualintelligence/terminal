/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    /// The default queue-based <see cref="ITerminalProcessor"/> responsible for processing command or batches in a
    /// terminal environment.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="TerminalProcessor"/> manages a queue of <see cref="TerminalRequest"/> items that are processed in
    /// the background. It routes these commands to the <see cref="ICommandRouter"/> for execution. The processor
    /// supports handling both single commands and batches of commands, as well as partial batches for single-client scenarios.
    /// </para>
    /// <para>
    /// Single commands are validated and enqueued individually for processing. For batch processing, the input string
    /// is split into individual commands using delimiters. Each command in the batch is validated and enqueued, with
    /// the order of commands within the batch being preserved during processing.
    /// </para>
    /// <para>
    /// In single-client scenarios, partial batches (incomplete commands) are stored internally and combined with
    /// subsequent input to form complete batches for continuous processing.
    /// </para>
    /// <para>
    /// The <see cref="TerminalProcessor"/> uses a <see cref="SemaphoreSlim"/> to synchronize access to the queue and
    /// signal the availability of items for processing, ensuring thread-safe command submissions from multiple clients.
    /// </para>
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

            streamingRequests = new ConcurrentDictionary<string, StringBuilder>();
            batchDelimiter = textHandler.Encoding.GetBytes(terminalOptions.Value.Router.BatchDelimiter);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="TerminalProcessor"/> queue is running in the background.
        /// </summary>
        public bool IsBackground { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="TerminalProcessor"/> is currently processing requests.
        /// </summary>
        public bool IsProcessing { get; private set; }

        /// <summary>
        /// Gets the collection of unprocessed requests at the time of query. It is a snapshot of the queue at the time
        /// of query and may not be accurate by the time the caller processes the collection.
        /// </summary>
        /// <remarks>
        /// THIS METHOD IS PART OF INTERNAL INFRASTRUCTURE AND IS NOT INTENDED TO BE USED BY APPLICATION CODE.
        /// </remarks>
        public IReadOnlyCollection<TerminalRequest> UnprocessedRequests
        {
            get
            {
                // Return all the requests from the unprocessed response queue.
                return unprocessedRequests.SelectMany(static r => r.Commands).ToArray();
            }
        }

        /// <summary>
        /// Asynchronously adds a terminal request for processing from a string.
        /// </summary>
        /// <param name="raw">The raw command or a batch to add to the processor.</param>
        /// <param name="senderId">The optional sender identifier.</param>
        /// <param name="senderEndpoint">The optional sender endpoint.</param>
        public Task AddRequestAsync(string raw, string? senderId, string? senderEndpoint)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new TerminalException(TerminalErrors.InvalidRequest, "The raw command or batch cannot be empty.");
            }

            if (raw.Length > terminalOptions.Value.Router.MaxLength)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The raw command or batch length exceeds configured maximum. max_length={0}", terminalOptions.Value.Router.MaxLength);
            }

            if (!IsProcessing)
            {
                throw new TerminalException(TerminalErrors.InvalidRequest, "The terminal processor is not running.");
            }

            if (!IsBackground)
            {
                throw new TerminalException(TerminalErrors.InvalidRequest, "The terminal processor is not running a background queue.");
            }

            string[] commands;
            bool isBatchEnabled = terminalOptions.Value.Router.EnableBatch.GetValueOrDefault();
            string? batchId = null;
            if (isBatchEnabled)
            {
                commands = ExtractBatchCommands(raw);
                batchId = NewUniqueId();
            }
            else
            {
                commands = [raw];
            }

            // Create a response object that will hold requests and results
            TerminalResponse response = new(commands.Length, batchId, senderId, senderEndpoint);
            for (int idx = 0; idx < commands.Length; ++idx)
            {
                response.Commands[idx] = new TerminalRequest(NewUniqueId(), commands[idx]);
            }

            // Enqueue and signal that a request is ready to be processed
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
        public string NewUniqueId(string? hint = null)
        {
            if (hint == "short")
            {
                return Guid.NewGuid().ToString("N").Substring(0, 12);
            }
            else
            {
                return Guid.NewGuid().ToString();
            }
        }

        /// <inheritdoc/>
        public async Task<TerminalResponse> ProcessRequestAsync(string raw, string? senderId, string? senderEndpoint)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new TerminalException(TerminalErrors.InvalidRequest, "The command or batch cannot be empty.");
            }

            if (raw.Length > terminalOptions.Value.Router.MaxLength)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The batch length exceeds configured maximum. max_length={0}", terminalOptions.Value.Router.MaxLength);
            }

            if (!IsProcessing || terminalRouterContext == null)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The terminal processor is not running.");
            }

            string[] commands;
            bool isBatchEnabled = terminalOptions.Value.Router.EnableBatch.GetValueOrDefault();
            string? batchId = null;
            if (isBatchEnabled)
            {
                commands = ExtractBatchCommands(raw);
                batchId = NewUniqueId();
            }
            else
            {
                commands = [raw];
            }

            TerminalResponse response = new(commands.Length, batchId, senderId, senderEndpoint);
            for (int idx = 0; idx < commands.Length; ++idx)
            {
                TerminalRequest request = new(NewUniqueId(), commands[idx]);

                CommandRouterResult result = await RouteRequestAsync(request, response, terminalRouterContext);
                object? value = result.HandlerResult.RunnerResult.HasValue ? result.HandlerResult.RunnerResult.Value : null;

                response.Commands[idx] = request;
                response.Results[idx] = value;
            }

            return response;
        }

        /// <summary>
        /// Serializes the result of a command execution to a JSON UTF8 byte array.
        /// </summary>
        /// <param name="result">The result to serialize.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public byte[] SerializeToJsonBytes(object? result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result), "The command runner result is null.");
            }

            return JsonSerializer.SerializeToUtf8Bytes(result);
        }

        /// <summary>
        /// Serializes the results into a JSON string.
        /// </summary>
        /// <param name="results">The results to serialize.</param>
        /// <returns></returns>
        public string SerializeToJsonString(object?[] results)
        {
            return JsonSerializer.Serialize(results);
        }

        /// <inheritdoc/>
        public void StartProcessing(TerminalRouterContext terminalRouterContext, bool background, Func<TerminalResponse, Task>? responseHandler = null)
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
        public async Task StreamRequestAsync(byte[] potential, string senderId, string? senderEndpoint)
        {
            // Get or create a StringBuilder for the sender
            StringBuilder senderBuffer = streamingRequests.GetOrAdd(senderId, static _ => new StringBuilder());

            // Append the new chunk to the existing buffer
            senderBuffer.Append(textHandler.Encoding.GetString(potential));
            var batchString = senderBuffer.ToString();

            // Check if the batch ends with the delimiter
            bool isPartial = !batchString.EndsWith(terminalOptions.Value.Router.BatchDelimiter);

            // Split the string by the batch delimiter
            string[] batches = batchString.Split(new[] { terminalOptions.Value.Router.BatchDelimiter }, StringSplitOptions.RemoveEmptyEntries);

            // Process each batch
            for (int idx = 0; idx < batches.Length; ++idx)
            {
                // Check if it's the last batch and is incomplete
                if (isPartial && idx == batches.Length - 1)
                {
                    // Save the partial batch back to the buffer
                    senderBuffer.Clear();
                    senderBuffer.Append(batches[idx]);
                    break;
                }

                // Send the complete batch for processing
                await AddRequestAsync(batches[idx] + terminalOptions.Value.Router.BatchDelimiter, senderId, senderEndpoint);
            }

            // Clear the buffer if all data was processed
            if (!isPartial)
            {
                senderBuffer.Clear();
            }
        }

        /// <summary>
        /// Starts a <see cref="Task.Delay(int, CancellationToken)"/> indefinitely until canceled.
        /// </summary>
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

        private string[] ExtractBatchCommands(string raw)
        {
            // Find the index of the batch delimiter in the raw input
            int firstIndex = raw.IndexOf(terminalOptions.Value.Router.BatchDelimiter, textHandler.Comparison);
            int delimiterLength = terminalOptions.Value.Router.BatchDelimiter.Length;

            // Check if the raw input is shorter than the delimiter length
            if (raw.Length < delimiterLength)
            {
                throw new TerminalException("invalid_request", "The raw batch does not end with the batch delimiter.");
            }

            // Check if the batch delimiter is not found or not at the end of the raw input
            if (firstIndex == -1 || firstIndex != (raw.Length - delimiterLength))
            {
                throw new TerminalException("invalid_request", "The raw batch must have a single delimiter at the end, not missing or placed elsewhere.");
            }

            return raw.Split(new[] { terminalOptions.Value.Router.CommandDelimiter, terminalOptions.Value.Router.BatchDelimiter }, StringSplitOptions.RemoveEmptyEntries);
        }

        private void RegisterResponseHandler(Func<TerminalResponse, Task> handler)
        {
            if (!terminalOptions.Value.Router.EnableResponses.GetValueOrDefault())
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The response handling is not enabled.");
            }

            this.handler = handler ?? throw new TerminalException(TerminalErrors.InvalidRequest, "The response handler cannot be null.");
        }

        private async Task<CommandRouterResult> RouteRequestAsync(TerminalRequest request, TerminalResponse terminalResponse, TerminalRouterContext terminalRouterContext)
        {
            Dictionary<string, object> properties = new()
            {
                { TerminalIdentifiers.SenderEndpointToken, terminalResponse.SenderEndpoint ?? "$unknown$" }
            };

            if (terminalResponse.SenderId != null)
            {
                properties.Add(TerminalIdentifiers.SenderIdToken, terminalResponse.SenderId);
            }

            logger.LogDebug("Routing the command. raw={0} sender={1}", request.Raw, terminalResponse.SenderId ?? "$unknown$");
            var context = new CommandRouterContext(request, terminalRouterContext, properties);
            var routeTask = commandRouter.RouteCommandAsync(context);

            if (await Task.WhenAny(routeTask, Task.Delay(terminalOptions.Value.Router.Timeout, terminalRouterContext.StartContext.TerminalCancellationToken)) == routeTask)
            {
                return await routeTask;
            }
            else
            {
                throw new TimeoutException($"The command router timed out in {terminalOptions.Value.Router.Timeout} milliseconds.");
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

            bool responseEnabled = terminalOptions.Value.Router.EnableResponses.GetValueOrDefault();

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
                        unprocessedRequests.TryDequeue(out TerminalResponse? response);
                        if (response != null)
                        {
                            for (int idx = 0; idx < response.Commands.Length; ++idx)
                            {
                                var result = await RouteRequestAsync(response.Commands[idx], response, terminalRouterContext);
                                object? value = result.HandlerResult.RunnerResult.HasValue ? result.HandlerResult.RunnerResult.Value : null;
                                response.Results[idx] = value;
                            }

                            // Request is processed and results are populated in the response, not push it to processed requests.
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
            if (!terminalOptions.Value.Router.EnableResponses.GetValueOrDefault())
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
        private readonly Stack<TerminalResponse> processedRequests;
        private readonly SemaphoreSlim requestSignal;
        private readonly SemaphoreSlim responseSignal;
        private readonly ConcurrentDictionary<string, StringBuilder> streamingRequests;
        private readonly ITerminalExceptionHandler terminalExceptionHandler;
        private readonly IOptions<TerminalOptions> terminalOptions;
        private readonly ITerminalTextHandler textHandler;
        private readonly ConcurrentQueue<TerminalResponse> unprocessedRequests;
        private byte[] batchDelimiter;
        private Func<TerminalResponse, Task>? handler;
        private Task requestProcessing;
        private Task responseProcessing;
        private TerminalRouterContext? terminalRouterContext;
    }
}
