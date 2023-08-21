/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Commands
{
    /// <summary>
    /// Represents an extracted command.
    /// </summary>
    public sealed class ExtractedCommand
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="hierarchy">The command hierarchy.</param>
        public ExtractedCommand(Command command, Root hierarchy)
        {
            Command = command;
            Hierarchy = hierarchy;
        }

        /// <summary>
        /// The command.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        /// The command hierarchy.
        /// </summary>
        public Root Hierarchy { get; }
    }
}