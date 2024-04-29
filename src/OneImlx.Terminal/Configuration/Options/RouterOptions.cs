/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

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
        /// The terminal caret to show in the console. The default value is <c>&gt;</c>.
        /// </summary>
        public string Caret { get; set; } = ">";

        /// <summary>
        /// Indicates whether to enable the remote delimiters. The actual delimiter values can be set by
        /// <seealso cref="RemoteCommandDelimiter"/> and <see cref="RemoteMessageDelimiter"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When enabled, remote senders can transmit multiple commands to the server as a single message, with each
        /// command separated by a delimiter. It's important to note that even if only a single command is sent, the
        /// message must be delimited for consistent message processing on the server side.
        /// </para>
        /// <para>To construct a correct a delimited message, use the <see cref="TerminalServices.DelimitedMessage(TerminalOptions, string[])"/></para>
        /// </remarks>
        /// <seealso cref="RemoteCommandDelimiter"/>
        /// <seealso cref="RemoteMessageDelimiter"/>
        public bool? EnableRemoteDelimiters { get; set; }

        /// <summary>
        /// The maximum number of active remote client connections the router can accept. The default value is <c>5</c>.
        /// </summary>
        public int MaxRemoteClients { get; set; } = 5;

        /// <summary>
        /// The delimiter to identify a complete command. The default value is <c>$c$</c>.
        /// </summary>
        /// <remarks>
        /// A <see cref="RemoteCommandDelimiter"/> is used while streaming a long command string from a remote source
        /// such as a network stream.
        /// </remarks>
        public string RemoteCommandDelimiter { get; set; } = TerminalIdentifiers.RemoteCommandDelimiter;

        /// <summary>
        /// The delimiter to identify a complete message. The default value is <c>$m$</c>.
        /// </summary>
        /// <remarks>
        /// A <see cref="RemoteMessageDelimiter"/> is used while streaming a multiple command string from a remote
        /// source such as a network stream.
        /// </remarks>
        public string RemoteMessageDelimiter { get; set; } = TerminalIdentifiers.RemoteMessageDelimiter;

        /// <summary>
        /// The maximum length of a single unprocessed remote message. The default value is <c>1024</c> characters.
        /// </summary>
        /// <remarks>
        /// This is not the actual command string length, but the length of the message that is being streamed from a
        /// remote source.
        /// </remarks>
        public int RemoteMessageMaxLength { get; set; } = 1024;

        /// <summary>
        /// The command router timeout in milliseconds. The default value is <c>25</c> seconds. Use
        /// <see cref="Timeout.Infinite"/> for infinite timeout.
        /// </summary>
        /// <remarks>
        /// A command route starts at a request to execute the command and ends when the command run is complete or at
        /// an error.
        /// </remarks>
        public int Timeout { get; set; } = 25000;
    }
}
