/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Commands
{
    /// <summary>
    /// An abstraction of a linked command.
    /// </summary>
    public interface ILinkedCommand
    {
        /// <summary>
        /// The linked command.
        /// </summary>
        public CommandDescriptor LinkedCommand { get; }
    }
}