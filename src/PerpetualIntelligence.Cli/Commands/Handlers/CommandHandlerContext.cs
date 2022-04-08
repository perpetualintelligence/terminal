/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Licensing;
using System;

namespace PerpetualIntelligence.Cli.Commands.Handlers
{
    /// <summary>
    /// The command handler context.
    /// </summary>
    public class CommandHandlerContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="commandDescriptor">The command descriptor to handle.</param>
        /// <param name="command">The command handle.</param>
        /// <param name="license">The extracted license.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public CommandHandlerContext(CommandDescriptor commandDescriptor, Command command, License license)
        {
            CommandDescriptor = commandDescriptor ?? throw new ArgumentNullException(nameof(commandDescriptor));
            Command = command ?? throw new ArgumentNullException(nameof(command));
            License = license ?? throw new ArgumentNullException(nameof(license));
        }

        /// <summary>
        /// The command to handle.
        /// </summary>
        public Command Command { get; protected set; }

        /// <summary>
        /// The command descriptor to handle.
        /// </summary>
        public CommandDescriptor CommandDescriptor { get; protected set; }

        /// <summary>
        /// The extracted licenses.
        /// </summary>
        public License License { get; protected set; }
    }
}
