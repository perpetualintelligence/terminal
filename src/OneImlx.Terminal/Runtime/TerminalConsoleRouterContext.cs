/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Collections.Generic;
using System.Threading;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The <see cref="TerminalRouterContext"/> for <see cref="TerminalConsoleRouter"/>.
    /// </summary>
    public sealed class TerminalConsoleRouterContext : TerminalRouterContext
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TerminalConsoleRouterContext"/>.
        /// </summary>
        /// <param name="startMode">The terminal start mode.</param>
        /// <param name="commandCancellationToken">The command router cancellation token.</param>
        /// <param name="routeOnce">Determines whether the router will route the request only once.</param>
        /// <param name="customProperties">The custom properties.</param>
        /// <param name="arguments">The command line arguments.</param>
        public TerminalConsoleRouterContext(
            TerminalStartMode startMode,
            CancellationToken commandCancellationToken,
            bool? routeOnce = null,
            Dictionary<string, object>? customProperties = null,
            string[]? arguments = null) : base(startMode, commandCancellationToken, customProperties, arguments)
        {
            RouteOnce = routeOnce;
        }

        /// <summary>
        /// Determines whether the router will route the request only once. When <see cref="RouteOnce"/> is set to
        /// <c>true</c>, the terminal router processes a single incoming request, executes the corresponding command,
        /// and then cancels further routing operations.
        /// </summary>
        /// <remarks>
        /// The route-once behavior is useful for scenarios like command-line tools or driver terminal programs where
        /// only one execution cycle is required. For example, in a .NET CLI, a command such as <c>dotnet build</c> runs
        /// once per invocation when executed from an existing terminal. Similarly, setting <see cref="RouteOnce"/> to
        /// <c>true</c> ensures the router processes one command, completes its task, and exits cleanly. If set to
        /// <c>false</c> (or left unset), the router continues processing incoming requests until explicitly canceled or terminated.
        /// </remarks>
        public bool? RouteOnce { get; }
    }
}
