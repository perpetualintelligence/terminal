/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace PerpetualIntelligence.Terminal.Commands.Checkers
{
    /// <summary>
    /// The command checker context.
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