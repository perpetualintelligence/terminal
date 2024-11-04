/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// The default queue-based <see cref="ITerminalProcessor"/> responsible for processing command batches in a
    /// terminal environment.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="TerminalProcessor"/> manages a queue of <see cref="TerminalProcessorRequest"/> items that are
    /// processed in the background. It routes these commands to the <see cref="ICommandRouter"/> for execution. The
    /// processor supports handling both single commands and batches of commands, as well as partial batches for
    /// single-client scenarios.
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

            partialBatchBuilder = new StringBuilder();
            requestQueue = new Queue<TerminalProcessorRequest>();
            requestProcessing = Task.CompletedTask;
            responseProcessing = Task.CompletedTask;
            queueSignal = new SemaphoreSlim(0);
            queueLock = new SemaphoreSlim(1);
            counters = new ulong[3];
        }

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
        public IReadOnlyCollection<TerminalProcessorRequest> UnprocessedRequests
        {
            get
            {
                queueLock.Wait();
                try
                {
                    return requestQueue.ToArray();
                }
                finally
                {
                    queueLock.Release();
                }
            }
        }

        /// <summary>
        /// Asynchronously adds a terminal request for processing from a string.
        /// </summary>
        /// <param name="potential">The potential command or a batch to add to the processor.</param>
        /// <param name="senderEndpoint">The optional sender endpoint.</param>
        /// <param name="senderId">The optional sender identifier.</param>
        public async Task AddAsync(string potential, string? senderEndpoint, string? senderId)
        {
            if (string.IsNullOrWhiteSpace(potential))
            {
                throw new TerminalException(TerminalErrors.InvalidRequest, "The command or batch cannot be empty.");
            }

            if (potential.Length > terminalOptions.Value.Router.RemoteBatchMaxLength)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The batch length exceeds configured maximum. max_length={0}", terminalOptions.Value.Router.RemoteBatchMaxLength);
            }

            bool isBatchEnabled = terminalOptions.Value.Router.EnableRemoteDelimiters.GetValueOrDefault();
            if (isBatchEnabled)
            {
                await EnqueueBatchesConcurrentAsync(potential, senderEndpoint, senderId);
            }
            else
            {
                EnqueueCommandNonConcurrent(potential, batchId: null, senderEndpoint, senderId);
            }
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            queueSignal.Dispose();
            queueLock.Dispose();
            return new ValueTask(Task.CompletedTask);
        }

        /// <inheritdoc/>
        public string NewUniqueId(string? hint = null)
        {
            lock (counters)
            {
                switch (hint)
                {
                    case "batch":
                        return $"b{counters[0]++}";

                    case "client":
                        return $"c{counters[1]++}";

                    case "route":
                        return $"r{counters[2]++}";

                    default:
                        return Guid.NewGuid().ToString();
                }
            }
        }

        /// <summary>
        /// Starts background processing.
        /// </summary>
        /// <param name="terminalRouterContext">The terminal router context.</param>
        public void StartProcessing(TerminalRouterContext terminalRouterContext)
        {
            // IMPORTANT: We don't await so both request and response processing happens in the background.
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

        private async Task EnqueueBatchesConcurrentAsync(string potential, string? senderEndpoint, string? senderId)
        {
            // Check if the potential batch string ends with the batch delimiter. If it doesn't, the batch is incomplete
            // (i.e., a partial batch that may be combined with future input).
            bool isPartial = !potential.EndsWith(terminalOptions.Value.Router.RemoteBatchDelimiter, textHandler.Comparison);

            // If there is a previously stored partial batch, append the current potential batch to it. Otherwise, use
            // the current potential batch as is. This step ensures we try to form a complete batch whenever possible.
            string potentialAndPrevious = partialBatchBuilder.Length > 0 ? partialBatchBuilder.Append(potential).ToString() : potential;
            string[] potentialBatches = potentialAndPrevious.Split(new[] { terminalOptions.Value.Router.RemoteBatchDelimiter }, StringSplitOptions.RemoveEmptyEntries);
            partialBatchBuilder.Clear();

            // Determine the number of complete batches. If the current batch is partial, we skip processing the last
            // batch, storing it for future use.
            int batchLength = potentialBatches.Length;
            if (isPartial)
            {
                // Array is 0-based, so we need to decrement the length to get the last index.
                batchLength -= 1;
                partialBatchBuilder.Append(potentialBatches[batchLength]);
            }

            // Enqueue each complete batch for processing. The last partial batch is stored for future use and ignored
            // in this step.
            for (int idx = 0; idx < batchLength; ++idx)
            {
                // We need to lock the queue to ensure that the commands in a batch are processed in the order they were.
                await queueLock.WaitAsync();
                try
                {
                    string batchId = NewUniqueId("batch");
                    string[] commands = potentialBatches[idx].Split(new[] { terminalOptions.Value.Router.RemoteCommandDelimiter }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string cmd in commands)
                    {
                        EnqueueCommandNonConcurrent(cmd, batchId, senderEndpoint, senderId);
                    }
                }
                finally
                {
                    queueLock.Release();
                }
            }
        }

        /// <summary>
        /// Enqueues a parsed message for processing.
        /// </summary>
        private void EnqueueCommandNonConcurrent(string command, string? batchId, string? senderEndpoint, string? senderId)
        {
            var request = new TerminalProcessorRequest(NewUniqueId(), command, batchId, senderEndpoint, senderId);
            requestQueue.Enqueue(request);
            queueSignal.Release();
        }

        private async Task ProcessRawCommandAsync(TerminalProcessorRequest item, TerminalRouterContext terminalRouterContext)
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
                // The infinite while(true) enable a continuous processing of the command queue until canceled.
                while (true)
                {
                    try
                    {
                        // Wait until there is a signal or the cancellation is requested. The queueSignal is used to
                        // signal that there is a new item in the queue, at the same time we don't hog the CPU in the
                        // outer while loop.
                        await queueSignal.WaitAsync(terminalRouterContext.StartContext.TerminalCancellationToken);

                        // The queueLock is used to ensure that only one thread is accessing the queue at a time. This
                        // ensures the commands in a batch are processed in the order they were received.
                        await queueLock.WaitAsync();
                        try
                        {
                            if (requestQueue.Count > 0)
                            {
                                TerminalProcessorRequest item = requestQueue.Dequeue();
                                await ProcessRawCommandAsync(item, terminalRouterContext);
                            }
                        }
                        finally
                        {
                            queueLock.Release();
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
            });
        }

        private Task StartResponseProcessingAsync(TerminalRouterContext terminalRouterContext) => Task.CompletedTask;

        private readonly ICommandRouter commandRouter;
        private readonly ILogger logger;
        private readonly StringBuilder partialBatchBuilder;
        private readonly SemaphoreSlim queueLock;
        private readonly SemaphoreSlim queueSignal;
        private readonly Queue<TerminalProcessorRequest> requestQueue;
        private readonly ITerminalExceptionHandler terminalExceptionHandler;
        private readonly IOptions<TerminalOptions> terminalOptions;
        private readonly ITerminalTextHandler textHandler;
        private ulong[] counters;
        private Task requestProcessing;
        private Task responseProcessing;
    }
}
