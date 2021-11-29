/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The default command runner context.
    /// </summary>
    public class CommandRunnerContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="command">The command to run.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public CommandRunnerContext(Command command)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        /// <summary>
        /// The checked command to run.
        /// </summary>
        public Command Command { get; }
    }
}
