/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Commands
{
    /// <summary>
    /// Represents a root command.
    /// </summary>
    public sealed class Root
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="isDummy"></param>
        /// <param name="linkedCommand">The command linked to the root.</param>
        /// <param name="childGroup">The child group.</param>
        /// <param name="childSubCommand">The child sub-command.</param>
        public Root(bool isDummy, Command linkedCommand, Group? childGroup = null, SubCommand? childSubCommand = null)
        {
            LinkedCommand = linkedCommand ?? throw new System.ArgumentNullException(nameof(linkedCommand));

            if (linkedCommand.Descriptor.IsRoot)
            {
                throw new System.ArgumentException("The command is not a root.", nameof(linkedCommand));
            }
            ChildGroup = childGroup;
            ChildSubCommand = childSubCommand;
            IsDummy = isDummy;
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
        /// Indicates whether this root is a dummy root.
        /// </summary>
        public bool IsDummy { get; set; }
    }
}