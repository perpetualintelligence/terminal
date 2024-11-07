/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Commands.Parsers
{
    /// <summary>
    /// Represents a parsed command from a command route.
    /// </summary>
    public sealed class ParsedCommand
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="commandRoute">The command route.</param>
        /// <param name="command">The command.</param>
        /// <param name="hierarchy">The command hierarchy.</param>
        public ParsedCommand(TerminalProcessorRequest commandRoute, Command command, Root? hierarchy = null)
        {
            CommandRoute = commandRoute ?? throw new System.ArgumentNullException(nameof(commandRoute));
            Command = command ?? throw new System.ArgumentNullException(nameof(command));
            Hierarchy = hierarchy;
        }

        /// <summary>
        /// The command route.
        /// </summary>
        public TerminalProcessorRequest CommandRoute { get; }

        /// <summary>
        /// The parsed raw command.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        /// The parsed <see cref="Command"/> hierarchy.
        /// </summary>
        /// <seealso cref="ParserOptions.ParseHierarchy"/>
        public Root? Hierarchy { get; }
    }
}