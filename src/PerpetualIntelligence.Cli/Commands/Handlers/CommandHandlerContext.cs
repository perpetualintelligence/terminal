/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

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
        /// <exception cref="ArgumentNullException"></exception>
        public CommandHandlerContext(CommandDescriptor commandDescriptor, Command command)
        {
            CommandDescriptor = commandDescriptor ?? throw new ArgumentNullException(nameof(commandDescriptor));
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        /// <summary>
        /// The command to handle.
        /// </summary>
        public Command Command { get; protected set; }

        /// <summary>
        /// The command descriptor to handle.
        /// </summary>
        public CommandDescriptor CommandDescriptor { get; protected set; }
    }
}
