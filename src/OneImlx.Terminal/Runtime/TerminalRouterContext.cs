/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The <see cref="ITerminalRouter{TContext}"/> context.
    /// </summary>
    public abstract class TerminalRouterContext
    {
        /// <summary>
        /// The terminal driver arguments or command line arguments.
        /// </summary>
        /// <remarks>
        /// These arguments are the command line inputs provided at the application's main entry point when starting a
        /// terminal session via a driver or via <see cref="Process"/>. This allows for dynamic and flexible terminal
        /// session configuration, adapting to different operational parameters and user-defined settings. It's
        /// particularly useful for modifying startup behaviors and handling various runtime environments, ensuring the
        /// terminal adapts to specific needs and contexts.
        /// </remarks>
        public string[]? Arguments { get; }

        /// <summary>
        /// The custom properties.
        /// </summary>
        /// <remarks>
        /// A collection of key-value pairs that can be used to pass additional information or configuration settings to
        /// the terminal session. These properties offer a way to extend the functionality and behavior of the terminal
        /// without modifying the core implementation.
        /// </remarks>
        public Dictionary<string, object>? CustomProperties { get; }

        /// <summary>
        /// The terminal start mode.
        /// </summary>
        public TerminalStartMode StartMode { get; }

        /// <summary>
        /// The cancellation token to cancel the global terminal routing.
        /// </summary>
        /// <remarks>
        /// This cancellation token is used for global control over the entire terminal routing. When triggered, it
        /// cancels the entire terminal routing, effectively stopping the routing any further commands. This is
        /// particularly useful in scenarios where a complete shutdown of the terminal's operational context is
        /// required, such as during application termination or critical error handling.
        /// </remarks>
        public CancellationToken TerminalCancellationToken { get; internal set; }

        /// <summary>
        /// Initializes a new <see cref="TerminalRouterContext"/> instance.
        /// </summary>
        /// <param name="startMode">The terminal start mode.</param>
        /// <param name="customProperties">The custom properties.</param>
        /// <param name="arguments">The command line arguments.</param>
        protected TerminalRouterContext(
            TerminalStartMode startMode,
            Dictionary<string, object>? customProperties = null,
            string[]? arguments = null)
        {
            Arguments = arguments;
            CustomProperties = customProperties;
            StartMode = startMode;
        }
    }
}
