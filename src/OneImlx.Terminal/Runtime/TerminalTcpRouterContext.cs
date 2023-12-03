/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Net;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// The TCP connection data.
    /// </summary>
    public sealed class TerminalTcpRouterContext : TerminalRouterContext
    {
        /// <summary>
        /// The network IP endpoint server will connect. Used for <see cref="TerminalTcpRouter"/>.
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