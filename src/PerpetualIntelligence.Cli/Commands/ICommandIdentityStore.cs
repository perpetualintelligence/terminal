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
    /// An abstraction to lookup a <see cref="Command"/>.
    /// </summary>
    public interface ICommandIdentityStore
    {
        /// <summary>
        /// Attempts to finds a command by its id.
        /// </summary>
        /// <param name="id">The command id.</param>
        public Task<OneImlxTryResult<CommandIdentity>> TryFindByIdAsync(string id);

        /// <summary>
        /// Attempts to find a command by its name.
        /// </summary>
        /// <param name="name">The command name.</param>
        public Task<OneImlxTryResult<CommandIdentity>> TryFindByNameAsync(string name);

        /// <summary>
        /// Attempts to find a command from the input command string.
        /// </summary>
        public Task<OneImlxTryResult<CommandIdentity>> TryFindMatchAsync(string commandString);
    }
}
