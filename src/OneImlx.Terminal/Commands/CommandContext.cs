/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// The generic command router context.
    /// </summary>
    public sealed class CommandContext
    {
        /// <summary>
        /// The command string.
        /// </summary>
        /// <param name="request">The request to process.</param>
        /// <param name="context">The terminal routing context.</param>
        /// <param name="properties">The additional router properties.</param>
        public CommandContext(
            TerminalRequest request,
            TerminalRouterContext context,
            Dictionary<string, object>? properties)
        {
            TerminalContext = context ?? throw new ArgumentNullException(nameof(context));
            Properties = properties;
            Request = request ?? throw new ArgumentNullException(nameof(request));
        }

        /// <summary>
        /// The extracted license.
        /// </summary>
        public License? License { get; internal set; }

        /// <summary>
        /// The parsed command.
        /// </summary>
        public ParsedCommand? ParsedCommand { get; internal set; }

        /// <summary>
        /// The additional router properties.
        /// </summary>
        public Dictionary<string, object>? Properties { get; }

        /// <summary>
        /// The terminal request.
        /// </summary>
        public TerminalRequest Request { get; }

        /// <summary>
        /// The result of the command execution.
        /// </summary>
        public CommandResult? Result { get; internal set; }

        /// <summary>
        /// The terminal routing context.
        /// </summary>
        public TerminalRouterContext TerminalContext { get; }

        /// <summary>
        /// Ensures the license is available.
        /// </summary>
        /// <returns>The available license.</returns>
        /// <exception cref="TerminalException">Thrown when the license is not available.</exception>
        public License EnsureLicense()
        {
            if (License is null)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The license is not available.");
            }

            return License;
        }

        /// <summary>
        /// Ensures the parsed command is available.
        /// </summary>
        /// <returns>The available parsed command.</returns>
        /// <exception cref="TerminalException">Thrown when the parsed command is not available.</exception>
        public ParsedCommand EnsureParsedCommand()
        {
            if (ParsedCommand is null)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The parsed command is not available.");
            }

            return ParsedCommand;
        }

        /// <summary>
        /// Ensures the command is available.
        /// </summary>
        /// <returns>The available command.</returns>
        /// <exception cref="TerminalException">Thrown when the parsed command is not available.</exception>
        public Command EnsureCommand()
        {
            if (ParsedCommand is null || ParsedCommand.Command is null)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The command is not available.");
            }

            return ParsedCommand.Command;
        }

        /// <summary>
        /// Ensures the result is available.
        /// </summary>
        /// <returns>The available result.</returns>
        /// <exception cref="TerminalException">Thrown when the result is not available.</exception>
        public CommandResult EnsureResult()
        {
            if (Result is null)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The result is not available.");
            }

            return Result;
        }
    }
}
