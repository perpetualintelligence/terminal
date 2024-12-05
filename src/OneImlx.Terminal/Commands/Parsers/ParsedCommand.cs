/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Collections;
using System.Collections.Generic;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Commands.Parsers
{
    /// <summary>
    /// Represents a parsed command from a command request.
    /// </summary>
    public sealed class ParsedCommand
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="request">The command request.</param>
        /// <param name="command">The command.</param>
        /// <param name="hierarchy">The command hierarchy.</param>
        /// <param name="hierarchy1"></param>
        public ParsedCommand(TerminalRequest request, Command command, Root? hierarchy = null, IEnumerable<CommandDescriptor>? hierarchy1 = null)
        {
            CommandRoute = request ?? throw new System.ArgumentNullException(nameof(request));
            Command = command ?? throw new System.ArgumentNullException(nameof(command));
            Hierarchy = hierarchy;
            Hierarchy1 = hierarchy1;
        }

        /// <summary>
        /// The parsed raw command.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        /// The command request.
        /// </summary>
        public TerminalRequest CommandRoute { get; }

        /// <summary>
        /// The parsed <see cref="Command"/> hierarchy.
        /// </summary>
        /// <seealso cref="ParserOptions.ParseHierarchy"/>
        public Root? Hierarchy { get; }

        /// <summary>
        /// The parsed <see cref="Command"/> hierarchy.
        /// </summary>
        public IEnumerable<CommandDescriptor>? Hierarchy1 { get; }
    }
}
