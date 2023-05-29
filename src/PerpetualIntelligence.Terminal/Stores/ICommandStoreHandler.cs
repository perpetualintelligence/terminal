/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Shared.Infrastructure;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Stores
{
    /// <summary>
    /// A store handler to lookup a <see cref="CommandDescriptor"/>.
    /// </summary>
    public interface ICommandStoreHandler
    {
        /// <summary>
        /// Attempts to finds a <see cref="CommandDescriptor"/> by its id.
        /// </summary>
        /// <param name="id">The command id.</param>
        public Task<TryResultOrError<CommandDescriptor>> TryFindByIdAsync(string id);

        /// <summary>
        /// Attempts to find a a <see cref="CommandDescriptor"/> by its name.
        /// </summary>
        /// <param name="name">The command name.</param>
        public Task<TryResultOrError<CommandDescriptor>> TryFindByNameAsync(string name);

        /// <summary>
        /// Attempts to find a a <see cref="CommandDescriptor"/> by its prefix.
        /// </summary>
        public Task<TryResultOrError<CommandDescriptor>> TryFindByPrefixAsync(string prefix);

        /// <summary>
        /// Attempts to match a a <see cref="CommandDescriptor"/> by its prefix.
        /// </summary>
        public Task<TryResultOrError<CommandDescriptor>> TryMatchByPrefixAsync(string prefix);
    }
}
