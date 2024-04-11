/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Net;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The <see cref="TerminalUdpRouter"/> connection context.
    /// </summary>
    public sealed class TerminalUdpRouterContext : TerminalRouterContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="iPEndPoint">The network IP endpoint server will connect.</param>
        /// <param name="startContext">The terminal start context.</param>
        public TerminalUdpRouterContext(IPEndPoint iPEndPoint, TerminalStartContext startContext) : base(startContext)
        {
            IPEndPoint = iPEndPoint;
        }

        /// <summary>
        /// The network IP endpoint terminal will connect.
        /// </summary>
        public IPEndPoint IPEndPoint { get; private set; }
    }
}