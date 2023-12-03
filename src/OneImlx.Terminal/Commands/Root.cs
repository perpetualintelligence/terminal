/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Commands
{
    /// <summary>
    /// Represents a notional root command.
    /// </summary>
    /// <remarks>
    /// A <see cref="Root"/> always has a <see cref="LinkedCommand"/> of type <see cref="CommandType.Root"/>.
    /// </remarks>
    public sealed class Root
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="linkedCommand">The command linked to the root.</param>
        /// <param name="childGroup">The child group.</param>
        /// <param name="childSubCommand">The child sub-command.</param>
        public Root(Command linkedCommand, Group? childGroup = null, SubCommand? childSubCommand = null)
        {
            LinkedCommand = linkedCommand ?? throw new System.ArgumentNullException(nameof(linkedCommand));

            if (linkedCommand.Descriptor.Type != CommandType.Root)
            {
                throw new System.ArgumentException("The command is not a root.", nameof(linkedCommand));
            }
            ChildGroup = childGroup;
            ChildSubCommand = childSubCommand;
            IsDefault = false;
        }

        /// <summary>
        /// The immediate child group.
        /// </summary>
        public Group? ChildGroup { get; set; }

        /// <summary>
        /// The child command.
        /// </summary>
        public SubCommand? ChildSubCommand { get; set; }

        /// <summary>
        /// The linked command of this root.
        /// </summary>
        public Command LinkedCommand { get; }

        /// <summary>
        /// Indicates whether this root is a default root.
        /// </summary>
        public bool IsDefault { get; private set; }

        /// <summary>
        /// Returns the default root.
        /// </summary>
        /// <param name="subCommand">The child sub-command.</param>
        public static Root Default(SubCommand? subCommand = null)
        {
            Root root = new(new Command(new CommandDescriptor("default", "default", "Default root command.", CommandType.Root, CommandFlags.None)), childGroup: null, childSubCommand: subCommand)
            {
                IsDefault = true
            };
            return root;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return LinkedCommand.Id;
        }
    }
}