/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Stores
{
    /// <summary>
    /// A store handler to lookup a <see cref="CommandDescriptor"/>.
    /// </summary>
    public interface ICommandStoreHandler
    {
        /// <summary>
        /// Attempts to finds a <see cref="CommandDescriptor"/> by its id asynchronously.
        /// </summary>
        /// <param name="id">The command id.</param>
        public Task<CommandDescriptor> FindByIdAsync(string id);

        /// <summary>
        /// Returns all <see cref="CommandDescriptor"/>s asynchronously.
        /// </summary>
        /// <returns>A <see cref="Dictionary{TKey, TValue}"/> of command descriptors. </returns>
        public Task<ReadOnlyDictionary<string, CommandDescriptor>> AllAsync();
    }
}