/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Net;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// Represents a <see cref="TerminalRemoteMessageQueue"/> item received from a remote sender to be processed by the
    /// terminal router.
    /// </summary>
    public sealed class TerminalRemoteMessageItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalRemoteMessageItem"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the command item.</param>
        /// <param name="command">The command string to be processed.</param>
        /// <param name="senderEndpoint">The sender endpoint from which the command was sent.</param>
        /// <param name="senderId">The sender id if the multiple senders shares same endpoint.</param>
        public TerminalRemoteMessageItem(string id, string command, EndPoint senderEndpoint, string? senderId)
        {
            Id = id;
            CommandString = command;
            SenderId = senderId;
            SenderEndpoint = senderEndpoint;
        }

        /// <summary>
        /// Gets the command string that needs to be processed.
        /// </summary>
        public string CommandString { get; }

        /// <summary>
        /// Gets the unique identifier for the command item.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the endpoint of the sender who issued the command.
        /// </summary>
        public EndPoint SenderEndpoint { get; }

        /// <summary>
        /// Gets the sender id if the multiple senders shares same endpoint.
        /// </summary>
        public string? SenderId { get; }
    }
}
