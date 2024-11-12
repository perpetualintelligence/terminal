/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Configuration.Options
{
    /// <summary>
    /// The command router options.
    /// </summary>
    public sealed class RouterOptions
    {
        /// <summary>
        /// The delimiter to identify a complete batch. The default value is <c>$b$</c>.
        /// </summary>
        /// <remarks>
        /// A <see cref="BatchDelimiter"/> is used while streaming a multiple command string from a remote source such
        /// as a network stream.
        /// </remarks>
        public string BatchDelimiter { get; set; } = TerminalIdentifiers.RemoteBatchDelimiter;

        /// <summary>
        /// The terminal caret to show in the console. The default value is <c>&gt;</c>.
        /// </summary>
        public string Caret { get; set; } = ">";

        /// <summary>
        /// The delimiter to identify a complete command. The default value is <c>$c$</c>.
        /// </summary>
        /// <remarks>
        /// A <see cref="CommandDelimiter"/> is used while streaming a long command string from a remote source such as
        /// a network stream.
        /// </remarks>
        public string CommandDelimiter { get; set; } = TerminalIdentifiers.RemoteCommandDelimiter;

        /// <summary>
        /// Indicates whether to enable the command batch.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When enabled, remote senders can transmit multiple commands to the server as a single batch, with each
        /// command separated by a delimiter. It's important to note that even if only a single command is sent, the
        /// batch must be delimited for consistent command processing on the server side.
        /// </para>
        /// <para>To construct a correct a delimited batch, use the <see cref="TerminalServices.CreateBatch(TerminalOptions, string[])"/></para>
        /// </remarks>
        /// <seealso cref="CommandDelimiter"/>
        /// <seealso cref="BatchDelimiter"/>
        public bool? EnableBatch { get; set; }

        /// <summary>
        /// Indicates whether to enable responses for the router. The default value is <c>false</c>.
        /// </summary>
        public bool? EnableResponses { get; set; }

        /// <summary>
        /// The maximum number of active remote client connections the router can accept. The default value is <c>5</c>.
        /// </summary>
        public int MaxClients { get; set; } = 5;

        /// <summary>
        /// The maximum length of a single unprocessed command or batch. The default value is <c>1024</c> characters.
        /// </summary>
        /// <remarks>
        /// This is not the actual command string length, but the length of the batch that is being streamed from a
        /// remote source.
        /// </remarks>
        public int MaxLength { get; set; } = 1024;

        /// <summary>
        /// The command router timeout in milliseconds. The default value is <c>25</c> seconds. Use
        /// <see cref="Timeout.Infinite"/> for infinite timeout.
        /// </summary>
        /// <remarks>
        /// A command request starts at a request to execute the command and ends when the command run is complete or at
        /// an error.
        /// </remarks>
        public int Timeout { get; set; } = 25000;
    }
}
