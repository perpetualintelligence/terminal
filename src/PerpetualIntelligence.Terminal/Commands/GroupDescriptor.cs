/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Commands
{
    /// <summary>
    /// Represents a command group descriptor.
    /// </summary>
    public sealed class GroupDescriptor : IChildCommand, ILinkedCommand
    {
        /// <summary>
        /// The immediate child group.
        /// </summary>
        public GroupDescriptor? ChildGroup { get; set; }

        /// <summary>
        /// The child command.
        /// </summary>
        public CommandDescriptor? ChildCommand { get; set; }

        /// <summary>
        /// The linked command of this root.
        /// </summary>
        public CommandDescriptor LinkedCommand { get; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="linkedCommand">The command linked to the root.</param>
        /// <param name="childGroup">The child group.</param>
        /// <param name="childCommand">The child command.</param>
        public GroupDescriptor(CommandDescriptor linkedCommand, GroupDescriptor? childGroup = null, CommandDescriptor? childCommand = null)
        {
            LinkedCommand = linkedCommand ?? throw new System.ArgumentNullException(nameof(linkedCommand));

            if (linkedCommand.IsGroup)
            {
                throw new System.ArgumentException("The command is not a group.", nameof(linkedCommand));
            }
            ChildGroup = childGroup;
            ChildCommand = childCommand;
        }
    }
}