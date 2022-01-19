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
    /// A store abstraction to lookup a <see cref="CommandIdentity"/>.
    /// </summary>
    public interface ICommandIdentityStore
    {
        /// <summary>
        /// Attempts to finds a <see cref="CommandIdentity"/> by its id.
        /// </summary>
        /// <param name="id">The command id.</param>
        public Task<OneImlxTryResult<CommandIdentity>> TryFindByIdAsync(string id);

        /// <summary>
        /// Attempts to find a a <see cref="CommandIdentity"/> by its name.
        /// </summary>
        /// <param name="name">The command name.</param>
        public Task<OneImlxTryResult<CommandIdentity>> TryFindByNameAsync(string name);

        /// <summary>
        /// Attempts to find a a <see cref="CommandIdentity"/> by its prefix.
        /// </summary>
        public Task<OneImlxTryResult<CommandIdentity>> TryFindByPrefixAsync(string prefix);        
    }
}
