/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Stores
{
    /// <summary>
    /// A store of <see cref="CommandDescriptor"/>.
    /// </summary>
    public interface ITerminalCommandStore
    {
        /// <summary>
        /// Adds a <see cref="CommandDescriptor"/> to the store.
        /// </summary>
        /// <param name="id">The command id.</param>
        /// <param name="commandDescriptor">The command descriptor to add.</param>
        /// <returns><c>true</c> if added, <c>false</c> otherwise.</returns>
        public Task<bool> TryAddAsync(string id, CommandDescriptor commandDescriptor);

        /// <summary>
        /// Attempts to finds a <see cref="CommandDescriptor"/> by its id asynchronously.
        /// </summary>
        /// <param name="id">The command id.</param>
        /// <param name="commandDescriptor">The <see cref="CommandDescriptor"/> if found, otherwise <c>null</c> </param>
        /// <remarks>
        /// This method should never throw an exception. If the command is not found then return <c>false</c>.
        /// </remarks>
        public Task<bool> TryFindByIdAsync(string id, out CommandDescriptor? commandDescriptor);

        /// <summary>
        /// Returns all <see cref="CommandDescriptor"/> asynchronously.
        /// </summary>
        /// <returns>A <see cref="Dictionary{TKey, TValue}"/> of command descriptors. </returns>
        public Task<CommandDescriptors> AllAsync();
    }
}