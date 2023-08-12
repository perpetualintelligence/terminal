/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Commands
{
    /// <summary>
    /// An abstraction that may have child command.
    /// </summary>
    public interface IChildCommand
    {
        /// <summary>
        /// The child command.
        /// </summary>
        public CommandDescriptor? ChildCommand { get; }
    }
}