/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Net;
using System.Text.Json.Serialization;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// A request that is processed by the a <see cref="ITerminalProcessor"/>.
    /// </summary>
    public sealed class TerminalProcessorRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalProcessorRequest"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the command item.</param>
        /// <param name="raw">The raw command string to be processed.</param>
        /// <param name="batchId">The batch identifier.</param>
        /// <param name="senderEndpoint">The sender endpoint from which the command was sent.</param>
        /// <param name="senderId">The sender id if the multiple senders shares same endpoint.</param>
        public TerminalProcessorRequest(string id, string raw, string? batchId, string? senderEndpoint, string? senderId)
        {
            Id = id;
            Raw = raw;
            BatchId = batchId;
            SenderId = senderId;
            SenderEndpoint = senderEndpoint;
        }

        /// <summary>
        /// The batch the command string is part of.
        /// </summary>
        public string? BatchId { get; }

        /// <summary>
        /// Gets the unique identifier for the command item.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The raw command or a batch that needs to be processed.
        /// </summary>
        public string Raw { get; }

        /// <summary>
        /// Gets the endpoint of the sender who issued the command.
        /// </summary>
        public string? SenderEndpoint { get; }

        /// <summary>
        /// Gets the sender id if the multiple senders shares same endpoint.
        /// </summary>
        public string? SenderId { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (BatchId != null)
            {
                return $"{BatchId} | {Raw}";
            }
            else
            {
                return $"{Raw}";
            }
        }
    }
}
