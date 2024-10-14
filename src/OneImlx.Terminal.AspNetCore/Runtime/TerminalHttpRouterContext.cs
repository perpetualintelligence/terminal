/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Runtime;
using System.Net;

namespace OneImlx.Terminal.AspNetCore.Runtime
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
        /// <param name="startContext">The terminal start context.</param>
        public TerminalHttpRouterContext(IPEndPoint iPEndPoint, TerminalStartContext startContext) : base(startContext)
        {
            IPEndPoint = iPEndPoint;
        }

        /// <summary>
        /// The IP endpoint for the <see cref="HttpListener"/>. Clients need to send messages to this endpoint.
        /// </summary>
        public IPEndPoint IPEndPoint { get; private set; }
    }
}
