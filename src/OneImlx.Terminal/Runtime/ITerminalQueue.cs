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

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// Represents a queue for processing terminal requests.
    /// </summary>
    public interface ITerminalQueue
    {
        /// <summary>
        /// Gets the current count of requests in the queue.
        /// </summary>
        int RequestCount { get; }

        /// <summary>
        /// Enqueues a terminal request for processing.
        /// </summary>
        /// <param name="message">The message to enqueue.</param>
        /// <param name="senderEndpoint">The sender's endpoint.</param>
        /// <param name="senderId">The sender's ID.</param>
        /// <returns>An enumerable of <see cref="TerminalQueueRequest"/> representing the enqueued requests.</returns>
        IEnumerable<TerminalQueueRequest> Enqueue(string message, string? senderEndpoint, string? senderId);

        /// <summary>
        /// Starts background processing of the queue.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        Task StartBackgroundProcessingAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Stops the background processing of the queue.
        /// </summary>
        Task StopBackgroundProcessingAsync(TimeSpan timeout);
    }
}
