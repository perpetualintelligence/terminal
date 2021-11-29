/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The default command checker context.
    /// </summary>
    public class CommandCheckerContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="command">The command to check.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public CommandCheckerContext(Command command)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        /// <summary>
        /// The command to check.
        /// </summary>
        public Command Command { get; set; }
    }
}
