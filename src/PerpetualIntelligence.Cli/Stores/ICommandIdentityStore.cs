/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Infrastructure;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands
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

        /// <summary>
        /// Attempts to match a <see cref="CommandIdentity"/> by its prefix.
        /// </summary>
        /// <remarks>
        /// This method enumerates all the <see cref="CommandIdentity"/> and attempts to find a first match where the
        /// specified command string starts with the <see cref="CommandIdentity.Prefix"/>.
        /// </remarks>
        public Task<OneImlxTryResult<CommandIdentity>> TryMatchByPrefixAsync(string commandString);
    }
}
