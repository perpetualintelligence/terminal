/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Oidc;
using PerpetualIntelligence.Shared.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The default command store.
    /// </summary>
    public class InMemoryCommandIdentityStore : ICommandIdentityStore
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="commands">The commands.</param>
        public InMemoryCommandIdentityStore(IEnumerable<CommandIdentity> commands)
        {
            Commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        /// <inheritdoc/>
        public Task<OneImlxTryResult<CommandIdentity>> TryFindByIdAsync(string id)
        {
            OneImlxTryResult<CommandIdentity> result = new();

            var command = Commands.FirstOrDefault(e => id.Equals(e.Id));
            if (command == null)
            {
                result.SetError(Errors.InvalidRequest, "The command is not valid.");
            }
            else
            {
                // Why identity just return the command?
                result.Result = new CommandIdentity("", "", null);
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<OneImlxTryResult<CommandIdentity>> TryFindByNameAsync(string name)
        {
            OneImlxTryResult<CommandIdentity> result = new();

            var command = Commands.FirstOrDefault(e => name.Equals(e.Name));
            if (command == null)
            {
                result.SetError(Errors.InvalidRequest, "The command name is not valid.");
            }
            else
            {
                // Why identity just return the command?
                result.Result = new CommandIdentity("", "", null);
            }

            return Task.FromResult(result);
        }

        public Task<OneImlxTryResult<CommandIdentity>> TryFindMatchAsync(string commandString)
        {
            OneImlxTryResult<CommandIdentity> result = new();

            // Find the command by name and group
            foreach (CommandIdentity command in Commands)
            {
                string path = command.Name;
                if (command.GroupId != null)
                {
                    path = $"{command.GroupId} {command.Name}";
                }

                if (commandString.StartsWith(path, StringComparison.OrdinalIgnoreCase))
                {
                    result.Result = command;
                    return Task.FromResult(result);
                }
            }

            result.SetError(Errors.InvalidRequest, "The path did not match with any command.");
            return Task.FromResult(result);
        }

        private IEnumerable<CommandIdentity> Commands { get; }
    }
}
