/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Collections.Generic;
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
        /// <param name="startMode">The terminal start mode.</param>
        /// <param name="customProperties">The custom properties.</param>
        /// <param name="arguments">The arguments.</param>
        public TerminalUdpRouterContext(
            IPEndPoint iPEndPoint,
            TerminalStartMode startMode,
            Dictionary<string, object>? customProperties = null,
            string[]? arguments = null)
            : base(startMode, customProperties, arguments)
        {
            IPEndPoint = iPEndPoint;
        }

        /// <summary>
        /// The network IP endpoint terminal will connect.
        /// </summary>
        public IPEndPoint IPEndPoint { get; private set; }
    }
}
