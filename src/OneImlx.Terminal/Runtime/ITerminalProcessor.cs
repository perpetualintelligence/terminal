/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Shared;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// An abstraction for processing the <see cref="TerminalInputOutput"/> individually or in batches, with optional
    /// asynchronous handling of responses in the background.
    /// </summary>
    /// <remarks>
    /// The <see cref="ITerminalProcessor"/> supports processing inputs, executing commands with responses, and managing
    /// streaming workflows efficiently in background tasks.
    /// </remarks>
    public interface ITerminalProcessor : IAsyncDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the processor is running in the background.
        /// </summary>
        bool IsBackground { get; }

        /// <summary>
        /// Gets a value indicating whether the processor is actively handling requests.
        /// </summary>
        bool IsProcessing { get; }

        /// <summary>
        /// Retrieves a snapshot of inputs that are pending processing.
        /// </summary>
        /// <remarks>
        /// The returned collection represents the state of unprocessed inputs at the time of retrieval. The actual
        /// state may change by the time the caller processes it.
        /// </remarks>
        IReadOnlyCollection<TerminalInputOutput> UnprocessedIOs { get; }

        /// <summary>
        /// Asynchronously adds a <see cref="TerminalInputOutput"/> for processing.
        /// </summary>
        /// <param name="terminalIO">The input to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddAsync(TerminalInputOutput terminalIO);

        /// <summary>
        /// Asynchronously executes the input and populates the output.
        /// </summary>
        /// <param name="terminalIO">The input to execute.</param>
        /// <remarks>
        /// The <see cref="ExecuteAsync"/> method processes the input immediately and returns the output. For background
        /// processing, use <see cref="AddAsync(TerminalInputOutput)"/>.
        /// </remarks>
        Task ExecuteAsync(TerminalInputOutput terminalIO);

        /// <summary>
        /// Starts processing terminal inputs with the specified context and configuration.
        /// </summary>
        /// <param name="terminalRouterContext">The context for the terminal router.</param>
        /// <param name="background">
        /// If <c>true</c>, the processor operates in the background, handling multiple requests asynchronously. If
        /// <c>false</c>, it processes individual requests and sends responses asynchronously.
        /// </param>
        /// <param name="responseHandler">An optional handler for processing responses.</param>
        void StartProcessing(TerminalRouterContext terminalRouterContext, bool background, Func<TerminalInputOutput, Task>? responseHandler = null);

        /// <summary>
        /// Attempts to stop background processing within a specified timeout period.
        /// </summary>
        /// <param name="timeout">The timeout duration in milliseconds.</param>
        /// <returns>
        /// A task representing the asynchronous operation, returning <c>true</c> if processing stopped within the
        /// timeout; otherwise, <c>false</c>.
        /// </returns>
        Task<bool> StopProcessingAsync(int timeout);

        /// <summary>
        /// Asynchronously streams a continuous flow of <see cref="TerminalInputOutput"/> as a byte array.
        /// </summary>
        /// <param name="bytes">The data to stream.</param>
        /// <param name="bytesLength">The length of the data to process. Use the total length to process all bytes.</param>
        /// <param name="senderId">The identifier of the sender.</param>
        /// <param name="senderEndpoint">An optional endpoint of the sender.</param>
        /// <remarks>
        /// The <see cref="StreamAsync(byte[], int, string, string?)"/> method is optimized for handling continuous
        /// streams of data. The <paramref name="bytesLength"/> parameter specifies the portion of the input to process,
        /// avoiding unnecessary duplication of data.
        /// </remarks>
        Task StreamAsync(byte[] bytes, int bytesLength, string senderId, string? senderEndpoint);

        /// <summary>
        /// Initiates a task that delays indefinitely until cancellation is requested.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to trigger termination.</param>
        Task WaitUntilCanceledAsync(CancellationToken cancellationToken);
    }
}
