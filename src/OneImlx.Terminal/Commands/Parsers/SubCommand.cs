/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace OneImlx.Terminal.Commands.Parsers
{
    /// <summary>
    /// Represents a notional sub-command within a <see cref="Group"/>.
    /// </summary>
    /// <remarks>
    /// A <see cref="SubCommand"/> always has a <see cref="LinkedCommand"/> of type <see cref="CommandType.SubCommand"/>.
    /// </remarks>
    public sealed class SubCommand
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="linkedCommand">The linked command.</param>
        public SubCommand(Command linkedCommand)
        {
            if (linkedCommand.Descriptor.Type != CommandType.SubCommand)
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