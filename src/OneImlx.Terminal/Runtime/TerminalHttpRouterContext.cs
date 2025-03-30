/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// Represents the context for the terminal router when running in HTTP mode.
    /// </summary>
    public sealed class TerminalHttpRouterContext : TerminalRouterContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalHttpRouterContext"/> class.
        /// </summary>
        /// <param name="iPEndPoint">The network IP endpoint for server connections.</param>
        /// <param name="startMode">The terminal start mode.</param>
        /// <param name="commandCancellationToken">The command router cancellation token.</param>
        /// <param name="customProperties">The custom properties.</param>
        /// <param name="arguments">The command line arguments.</param>
        public TerminalHttpRouterContext(
            IPEndPoint iPEndPoint,
            TerminalStartMode startMode,
            CancellationToken commandCancellationToken,
            Dictionary<string, object>? customProperties = null,
            string[]? arguments = null)
            : base(startMode, commandCancellationToken, customProperties, arguments)
        {
            IPEndPoint = iPEndPoint;
        }

        /// <summary>
        /// The IP endpoint for the <see cref="HttpListener"/>. Clients need to send messages to this endpoint.
        /// </summary>
        public IPEndPoint IPEndPoint { get; }
    }
}
