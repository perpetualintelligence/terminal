/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Net;
using System.Net.Sockets;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The <see cref="TerminalTcpRouter"/> connection context.
    /// </summary>
    public sealed class TerminalTcpRouterContext : TerminalRouterContext
    {
        /// <summary>
        /// The IP endpoint for the <see cref="TcpListener"/>. The clients need to send the messages to this end point.
        /// </summary>
        public IPEndPoint IPEndPoint { get; private set; }

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="iPEndPoint">The network IP endpoint server will connect.</param>
        /// <param name="terminalStartContext">The terminal start context.</param>
        public TerminalTcpRouterContext(IPEndPoint iPEndPoint, TerminalStartContext terminalStartContext) : base(terminalStartContext)
        {
            IPEndPoint = iPEndPoint;
        }
    }
}