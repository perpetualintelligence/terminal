/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Commands.Handlers
{
    /// <summary>
    /// The <c>oneimlx</c> generic command checker context.
    /// </summary>
    public class CommandHandlerContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="commandIdentity">The command identity to handle.</param>
        /// <param name="command">The command handle.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public CommandHandlerContext(CommandIdentity commandIdentity, Command command)
        {
            CommandIdentity = commandIdentity ?? throw new ArgumentNullException(nameof(commandIdentity));
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        /// <summary>
        /// The command to handle.
        /// </summary>
        public Command Command { get; protected set; }

        /// <summary>
        /// The command identity to handle.
        /// </summary>
        public CommandIdentity CommandIdentity { get; protected set; }
    }
}
