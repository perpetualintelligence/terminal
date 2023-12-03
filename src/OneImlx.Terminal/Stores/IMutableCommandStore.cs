/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Stores
{
    /// <summary>
    /// A mutable store of <see cref="CommandDescriptor"/>.
    /// </summary>
    public interface IMutableCommandStore : IImmutableCommandStore
    {
        /// <summary>
        /// Adds a <see cref="CommandDescriptor"/> to the store.
        /// </summary>
        /// <param name="id">The command id.</param>
        /// <param name="commandDescriptor">The command descriptor to add.</param>
        /// <returns><c>true</c> if added, <c>false</c> otherwise.</returns>
        public Task<bool> TryAddAsync(string id, CommandDescriptor commandDescriptor);
    }
}