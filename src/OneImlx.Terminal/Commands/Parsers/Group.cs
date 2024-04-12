/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace OneImlx.Terminal.Commands.Parsers
{
    /// <summary>
    /// Represents a notional command group  within a <see cref="Root"/>.
    /// </summary>
    /// <remarks>
    /// A <see cref="Group"/> always has a <see cref="LinkedCommand"/> of type <see cref="CommandType.Group"/>.
    /// </remarks>
    public sealed class Group
    {
        /// <summary>
        /// The immediate child group.
        /// </summary>
        public Group? ChildGroup { get; set; }

        /// <summary>
        /// The child sub-command.
        /// </summary>
        public SubCommand? ChildSubCommand { get; set; }

        /// <summary>
        /// The linked command of this group.
        /// </summary>
        public Command LinkedCommand { get; set; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="linkedCommand">The command linked to the root.</param>
        /// <param name="childGroup">The child group.</param>
        /// <param name="childSubCommand">The child sub-command.</param>
        public Group(Command linkedCommand, Group? childGroup = null, SubCommand? childSubCommand = null)
        {
            LinkedCommand = linkedCommand ?? throw new System.ArgumentNullException(nameof(linkedCommand));

            if (linkedCommand.Descriptor.Type != CommandType.Group)
            {
                throw new System.ArgumentException("The command is not a group.", nameof(linkedCommand));
            }

            ChildGroup = childGroup;
            ChildSubCommand = childSubCommand;
        }
    }
}