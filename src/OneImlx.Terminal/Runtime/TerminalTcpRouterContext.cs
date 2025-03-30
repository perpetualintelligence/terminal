/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The <see cref="TerminalTcpRouter"/> connection context.
    /// </summary>
    public sealed class TerminalTcpRouterContext : TerminalRouterContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="iPEndPoint">The network IP endpoint server will connect.</param>
        /// <param name="startMode">The terminal start mode.</param>
        /// <param name="commandCancellationToken">The command cancellation token.</param>
        /// <param name="customProperties">The custom properties.</param>
        /// <param name="arguments">The command line arguments.</param>
        public TerminalTcpRouterContext(
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
        /// The IP endpoint for the <see cref="TcpListener"/>. The clients need to send the messages to this end point.
        /// </summary>
        public IPEndPoint IPEndPoint { get; private set; }
    }
}
