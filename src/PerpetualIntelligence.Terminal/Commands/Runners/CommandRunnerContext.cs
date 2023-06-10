/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;

namespace PerpetualIntelligence.Terminal.Commands.Runners
{
    /// <summary>
    /// The command runner context.
    /// </summary>
    public sealed class CommandRunnerContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="command">The command to run.</param>
        /// <param name="custom">The custom data for the command to run.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public CommandRunnerContext(Command command, Dictionary<string, object>? custom = null)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Custom = custom;
        }

        /// <summary>
        /// The checked command to run.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        /// The custom data for the command.
        /// </summary>
        public Dictionary<string, object>? Custom { get; }
    }
}