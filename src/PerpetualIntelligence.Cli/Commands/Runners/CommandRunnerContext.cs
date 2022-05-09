/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Commands.Runners
{
    /// <summary>
    /// The command runner context.
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
