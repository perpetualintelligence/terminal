/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Stores
{
    /// <summary>
    /// The default in-memory <see cref="ICommandStoreHandler"/>.
    /// </summary>
    public class InMemoryCommandStore : ICommandStoreHandler
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="textHandler"></param>
        /// <param name="commandDescriptors">The command identities.</param>
        public InMemoryCommandStore(ITextHandler textHandler, IEnumerable<CommandDescriptor> commandDescriptors)
        {
            this.textHandler = textHandler;
            this.commandDescriptors = new CommandDescriptors(textHandler, commandDescriptors);
        }

        /// <summary>
        /// Returns all command descriptors asynchronously.
        /// </summary>
        /// <returns>A <see cref="ReadOnlyDictionary{TKey, TValue}"/> of command descriptors.</returns>
        public Task<ReadOnlyDictionary<string, CommandDescriptor>> AllAsync()
        {
            return Task.FromResult(new ReadOnlyDictionary<string, CommandDescriptor>(commandDescriptors));
        }

        /// <inheritdoc/>
        public Task<bool> TryFindByIdAsync(string id, out CommandDescriptor? commandDescriptor)
        {
            return Task.FromResult(commandDescriptors.TryGetValue(id, out commandDescriptor));
        }

        private readonly CommandDescriptors commandDescriptors;
        private readonly ITextHandler textHandler;
    }
}