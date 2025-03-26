﻿/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading;
using OneImlx.Terminal.Shared;

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
        /// Gets or sets a value indicating whether responses are disabled. The default value is <c>false</c>.
        /// </summary>
        /// <remarks>
        /// When disabled or set to <c>true</c>, the terminal server or host application operates in a
        /// request-processing-only mode. In this mode, the server executes incoming requests but does not send back any
        /// responses to the client.
        /// </remarks>
        public bool DisableResponse { get; set; }

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
        /// Define the route delay in milliseconds. The default value is <c>50</c> milliseconds.
        /// </summary>
        public int RouteDelay { get; set; } = 50;

        /// <summary>
        /// The command router timeout in milliseconds. The default value is <c>25</c> seconds. Use
        /// <see cref="Timeout.Infinite"/> for infinite timeout.
        /// </summary>
        /// <remarks>
        /// A command request starts at a request to execute the command and ends when the command run is complete or at
        /// an error.
        /// </remarks>
        public int Timeout { get; set; } = 25000;

        /// <summary>
        /// Represents the delimiter used to identify individual <see cref="TerminalInputOutput"/> within a continuous stream
        /// of bytes. The delimiter is set to <c>0x1E</c> (ASCII "Record Separator"), a non-printable control character
        /// commonly used to separate data and unlikely to appear in standard text.
        /// </summary>
        public byte StreamDelimiter = TerminalIdentifiers.StreamDelimiter;
    }
}
