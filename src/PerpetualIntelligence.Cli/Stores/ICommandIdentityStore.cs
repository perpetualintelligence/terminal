/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Shared.Infrastructure;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Stores
{
    /// <summary>
    /// A store abstraction to lookup a <see cref="CommandDescriptor"/>.
    /// </summary>
    public interface ICommandIdentityStore
    {
        /// <summary>
        /// Attempts to finds a <see cref="CommandDescriptor"/> by its id.
        /// </summary>
        /// <param name="id">The command id.</param>
        public Task<TryResult<CommandDescriptor>> TryFindByIdAsync(string id);

        /// <summary>
        /// Attempts to find a a <see cref="CommandDescriptor"/> by its name.
        /// </summary>
        /// <param name="name">The command name.</param>
        public Task<TryResult<CommandDescriptor>> TryFindByNameAsync(string name);

        /// <summary>
        /// Attempts to find a a <see cref="CommandDescriptor"/> by its prefix.
        /// </summary>
        public Task<TryResult<CommandDescriptor>> TryFindByPrefixAsync(string prefix);        
    }
}
