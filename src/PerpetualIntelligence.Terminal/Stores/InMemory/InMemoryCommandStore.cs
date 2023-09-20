/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Stores.InMemory
{
    /// <summary>
    /// The default in-memory <see cref="ICommandStoreHandler"/>.
    /// </summary>
    public class InMemoryCommandStore : ICommandStoreHandler
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="commandDescriptors">The command identities.</param>
        public InMemoryCommandStore(Dictionary<string, CommandDescriptor> commandDescriptors)
        {
            this.commandDescriptors = commandDescriptors ?? throw new ArgumentNullException(nameof(commandDescriptors));
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

        private readonly Dictionary<string, CommandDescriptor> commandDescriptors;
    }
}