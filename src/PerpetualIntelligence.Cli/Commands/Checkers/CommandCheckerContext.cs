/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    /// <summary>
    /// The command checker context.
    /// </summary>
    public class CommandCheckerContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="commandDescriptor">The command descriptor.</param>
        /// <param name="command">The command to check.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public CommandCheckerContext(CommandDescriptor commandDescriptor, Command command)
        {
            CommandDescriptor = commandDescriptor ?? throw new ArgumentNullException(nameof(commandDescriptor));
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        /// <summary>
        /// The command to check.
        /// </summary>
        public Command Command { get; set; }

        /// <summary>
        /// The command descriptor.
        /// </summary>
        public CommandDescriptor CommandDescriptor { get; }
    }
}
