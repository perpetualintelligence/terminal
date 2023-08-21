/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace PerpetualIntelligence.Terminal.Commands
{
    /// <summary>
    /// Represents a sub-command within a <see cref="Group"/>.
    /// </summary>
    public sealed class SubCommand
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="linkedCommand">The linked command.</param>
        public SubCommand(Command linkedCommand)
        {
            if (!linkedCommand.Descriptor.IsSubCommand)
            {
                throw new ArgumentException("The command is not a sub-command.", nameof(linkedCommand));
            }

            LinkedCommand = linkedCommand;
        }

        /// <summary>
        /// The linked command.
        /// </summary>
        public Command LinkedCommand { get; }
    }
}