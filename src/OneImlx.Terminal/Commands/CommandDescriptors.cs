/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Runtime;
using System.Collections.Generic;

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// The optimized <see cref="CommandDescriptor"/> collection.
    /// </summary>
    public sealed class CommandDescriptors : Dictionary<string, CommandDescriptor>
    {
        /// <summary>
        /// Initializes a new instance with the specified command descriptors.
        /// </summary>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="collection">The command descriptors.</param>
        public CommandDescriptors(ITerminalTextHandler textHandler, IEnumerable<CommandDescriptor> collection) : base(textHandler.EqualityComparer())
        {
            foreach (CommandDescriptor commandDescriptor in collection)
            {
                Add(commandDescriptor.Id, commandDescriptor);
            }
            TextHandler = textHandler;
        }

        /// <summary>
        /// The text handler.
        /// </summary>
        public ITerminalTextHandler TextHandler { get; }
    }
}