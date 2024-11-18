/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OneImlx.Terminal.Commands.Routers;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// An abstraction for processing command requests and optionally handling responses asynchronously in the background.
    /// </summary>
    public interface ITerminalProcessor : IAsyncDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the processor is running a queue in the background.
        /// </summary>
        bool IsBackground { get; }

        /// <summary>
        /// Gets a value indicating whether the processor is actively processing requests.
        /// </summary>
        bool IsProcessing { get; }

        /// <summary>
        /// Retrieves a collection of requests that are yet to be processed.
        /// </summary>
        /// <remarks>
        /// The returned collection is a snapshot at the time of the query and may not accurately reflect the state of
        /// the queue by the time it is processed by the caller.
        /// </remarks>
        IReadOnlyCollection<TerminalCommand> UnprocessedRequests { get; }

        /// <summary>
        /// Asynchronously adds a raw command or batch of commands to the processing queue.
        /// </summary>
        /// <param name="raw">The raw command or batch of commands to enqueue.</param>
        /// <param name="senderId">The optional identifier of the sender.</param>
        /// <param name="senderEndpoint">The optional endpoint of the sender.</param>
        /// <returns>A task representing the asynchronous operation, containing the request ID of the added request.</returns>
        Task AddRequestAsync(string raw, string? senderId, string? senderEndpoint);

        /// <summary>
        /// Generates a new unique identifier for use in request tracking.
        /// </summary>
        /// <param name="hint">An optional hint to customize the generated identifier.</param>
        /// <returns>A unique identifier string.</returns>
        string NewUniqueId(string? hint = null);

        /// <summary>
        /// Asynchronously processes a terminal request by executing a raw command or batch of commands and returning
        /// the responses.
        /// </summary>
        /// <param name="raw">The raw command or batch to be processed.</param>
        /// <param name="senderId">The optional identifier of the sender.</param>
        /// <param name="senderEndpoint">The optional endpoint of the sender.</param>
        /// <returns>A task representing the asynchronous operation, containing an array of <see cref="TerminalResponse"/>.</returns>
        /// <remarks>
        /// Commands in a batch are executed sequentially in the order they appear, and the corresponding responses are
        /// returned in the same order. This ensures consistency between the input commands and their results.
        /// </remarks>
        Task<TerminalResponse> ProcessRequestAsync(string raw, string? senderId, string? senderEndpoint);

        /// <summary>
        /// Serializes the results of a command execution to a UTF8 bytes.
        /// </summary>
        /// <param name="result">The result to serialize.</param>
        byte[] SerializeToJsonBytes(object? result);

        /// <summary>
        /// Serializes the results of a command execution to a string representation.
        /// </summary>
        /// <param name="results">The results to serialize.</param>
        string SerializeToJsonString(object?[] results);

        /// <summary>
        /// Starts the terminal processing with the specified context and configuration.
        /// </summary>
        /// <param name="terminalRouterContext">The context for the terminal router.</param>
        /// <param name="background">
        /// If set to <c>true</c>, the processor runs in the background, processing multiple requests asynchronously. If
        /// <c>false</c>, the processor handles individual requests and sends responses asynchronously.
        /// </param>
        /// <param name="responseHandler">The response handler.</param>
        void StartProcessing(TerminalRouterContext terminalRouterContext, bool background, Func<TerminalResponse, Task>? responseHandler = null);

        /// <summary>
        /// Attempts to stop the background processing within the specified timeout period.
        /// </summary>
        /// <param name="timeout">The timeout duration, in milliseconds.</param>
        /// <returns>
        /// A task representing the asynchronous operation, returning <c>true</c> if the processing stopped within the
        /// timeout; otherwise, <c>false</c>.
        /// </returns>
        Task<bool> StopProcessingAsync(int timeout);

        /// <summary>
        /// Asynchronously processes a raw command or batch of commands and returns the responses.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="senderId"></param>
        /// <param name="senderEndpoint"></param>
        /// <returns></returns>
        Task StreamRequestAsync(byte[] bytes, string senderId, string? senderEndpoint);

        /// <summary>
        /// Initiates an indefinite delay task that continues until the cancellation is triggered.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task WaitUntilCanceledAsync(CancellationToken cancellationToken);
    }
}
