/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Net;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// Represents a <see cref="TerminalCommandQueue"/> item received from a remote sender to be processed by the terminal.
    /// </summary>
    public sealed class TerminalCommandQueueItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalCommandQueueItem"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the command item.</param>
        /// <param name="command">The command string to be processed.</param>
        /// <param name="sender">The endpoint from which the command was sent.</param>
        public TerminalCommandQueueItem(string id, string command, IPEndPoint sender)
        {
            Id = id;
            Command = command;
            Sender = sender;
        }

        /// <summary>
        /// Gets the unique identifier for the command item.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the command string that needs to be processed.
        /// </summary>
        public string Command { get; }

        /// <summary>
        /// Gets the IP endpoint of the sender who issued the command.
        /// </summary>
        public IPEndPoint Sender { get; }
    }
}