/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The terminal start context.
    /// </summary>
    /// <remarks>
    /// The <see cref="TerminalStartContext"/> represents the context for starting a terminal session, including start mode, cancellation tokens,
    /// custom properties, and driver or command line arguments. It encapsulates all necessary information
    /// to initiate and control a terminal session, providing flexibility through custom properties and
    /// robust control via cancellation tokens.
    /// </remarks>
    public sealed class TerminalStartContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="terminalStartMode">The terminal start mode.</param>
        /// <param name="terminalCancellationToken">The terminal routing cancellation token.</param>
        /// <param name="commandCancellationToken">The command routing cancellation token.</param>
        /// <param name="customProperties">The custom properties.</param>
        /// <param name="driverArguments">The driver arguments or command line start arguments.</param>
        public TerminalStartContext(TerminalStartMode terminalStartMode, CancellationToken terminalCancellationToken, CancellationToken commandCancellationToken, Dictionary<string, object>? customProperties = null, string[]? driverArguments = null)
        {
            StartMode = terminalStartMode;
            TerminalCancellationToken = terminalCancellationToken;
            CommandCancellationToken = commandCancellationToken;
            CustomProperties = customProperties;
            Arguments = driverArguments;
        }

        /// <summary>
        /// The terminal driver arguments or command line arguments.
        /// </summary>
        /// <remarks>
        /// These arguments are the command line inputs provided at the application's main entry point when
        /// starting a terminal session via a driver or via <see cref="Process"/>. This allows for dynamic
        /// and flexible terminal session configuration, adapting to different operational parameters and
        /// user-defined settings. It's particularly useful for modifying startup behaviors and handling
        /// various runtime environments, ensuring the terminal adapts to specific needs and contexts.
        /// </remarks>
        public string[]? Arguments { get; }

        /// <summary>
        /// The terminal start mode.
        /// </summary>
        public TerminalStartMode StartMode { get; }

        /// <summary>
        /// The cancellation token to cancel the global terminal routing.
        /// </summary>
        /// <remarks>
        /// This cancellation token is used for global control over the entire terminal routing.
        /// When triggered, it cancels the entire terminal routing, effectively stopping the routing
        /// any further commands. This is particularly useful in scenarios where
        /// a complete shutdown of the terminal's operational context is required, such as during
        /// application termination or critical error handling.
        /// </remarks>

        public CancellationToken TerminalCancellationToken { get; }

        /// <summary>
        /// The cancellation token to cancel the current command routing.
        /// </summary>
        /// <remarks>
        /// This cancellation token is used for granular control over individual long-running commands.
        /// When triggered, it cancels only the currently executing command without affecting the overall
        /// terminal routing. This allows for selective interruption of commands, which is useful in scenarios
        /// where a specific operation needs to be stopped due to changes in user input or application state,
        /// while allowing the terminal to continue routing new commands.
        /// </remarks>
        public CancellationToken CommandCancellationToken { get; }

        /// <summary>
        /// The custom properties.
        /// </summary>
        /// <remarks>
        /// A collection of key-value pairs that can be used to pass additional information or configuration
        /// settings to the terminal session. These properties offer a way to extend the functionality
        /// and behavior of the terminal without modifying the core implementation.
        /// </remarks>
        public Dictionary<string, object>? CustomProperties { get; }
    }
}