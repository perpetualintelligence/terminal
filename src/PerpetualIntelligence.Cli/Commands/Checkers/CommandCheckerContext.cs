/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
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
        /// <param name="commandIdentity">The command identity.</param>
        /// <param name="command">The command to check.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public CommandCheckerContext(CommandIdentity commandIdentity, Command command)
        {
            CommandIdentity = commandIdentity ?? throw new ArgumentNullException(nameof(commandIdentity));
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        /// <summary>
        /// The command to check.
        /// </summary>
        public Command Command { get; set; }

        /// <summary>
        /// The command identity.
        /// </summary>
        public CommandIdentity CommandIdentity { get; }
    }
}
