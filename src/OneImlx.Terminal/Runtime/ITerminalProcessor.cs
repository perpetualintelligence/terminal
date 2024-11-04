/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// An abstraction to process command requests and responses in the background.
    /// </summary>
    public interface ITerminalProcessor : IAsyncDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the processor is currently processing requests.
        /// </summary>
        bool IsProcessing { get; }

        /// <summary>
        /// The unprocessed requests at the time of query.
        /// </summary>
        IReadOnlyCollection<TerminalProcessorRequest> UnprocessedRequests { get; }

        /// <summary>
        /// Asynchronously adds a terminal request for processing from a string.
        /// </summary>
        /// <param name="batch">The message to enqueue.</param>
        /// <param name="senderEndpoint">The sender's endpoint.</param>
        /// <param name="senderId">The sender's ID.</param>
        Task AddAsync(string batch, string? senderEndpoint, string? senderId);

        /// <summary>
        /// Generates a new unique identifier.
        /// </summary>
        /// <param name="hint">The hint to generate the unique identifier.</param>
        string NewUniqueId(string? hint = null);

        /// <summary>
        /// Starts background processing.
        /// </summary>
        /// <param name="terminalRouterContext">The terminal router context.</param>
        void StartProcessing(TerminalRouterContext terminalRouterContext);

        /// <summary>
        /// Attempts to stop the background processing asynchronously.
        /// </summary>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <returns><c>true</c> if the processing times out, <c>false</c> otherwise.</returns>
        Task<bool> StopProcessingAsync(int timeout);

        /// <summary>
        /// Starts a <see cref="Task.Delay(int, CancellationToken)"/> indefinitely until
        /// <see cref="TerminalStartContext.TerminalCancellationToken"/> is triggered.
        /// </summary>
        /// <param name="terminalRouterContext">The terminal router context.</param>
        Task WaitAsync(TerminalRouterContext terminalRouterContext);
    }
}
