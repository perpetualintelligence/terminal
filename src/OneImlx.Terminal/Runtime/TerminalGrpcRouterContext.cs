/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Collections.Generic;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// Represents the context for configuring and managing the gRPC router in the terminal.
    /// </summary>
    /// <remarks>
    /// This context is used to configure all gRPC server settings including server credentials, host address, and port.
    /// All necessary services for the gRPC server are added through this context.
    /// </remarks>
    public class TerminalGrpcRouterContext : TerminalRouterContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalGrpcRouterContext"/> class.
        /// </summary>
        /// <param name="startMode">The terminal start mode.</param>
        /// <param name="customProperties">The custom properties.</param>
        /// <param name="arguments">The command line arguments.</param>
        public TerminalGrpcRouterContext(
            TerminalStartMode startMode,
            Dictionary<string, object>? customProperties = null,
            string[]? arguments = null) : base(startMode, customProperties, arguments)
        {
        }
    }
}
