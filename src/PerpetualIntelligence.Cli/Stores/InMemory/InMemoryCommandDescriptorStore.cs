/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Stores;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Shared.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Stores.InMemory
{
    /// <summary>
    /// The default in-memory <see cref="ICommandDescriptorStore"/>.
    /// </summary>
    public class InMemoryCommandDescriptorStore : ICommandDescriptorStore
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="commandIdentities">The command identities.</param>
        public InMemoryCommandDescriptorStore(IEnumerable<CommandDescriptor> commandIdentities)
        {
            this.commandIdentities = commandIdentities ?? throw new ArgumentNullException(nameof(commandIdentities));
        }

        /// <inheritdoc/>
        public Task<TryResultOrError<CommandDescriptor>> TryFindByIdAsync(string id)
        {
            var command = commandIdentities.FirstOrDefault(e => id.Equals(e.Id));
            if (command == null)
            {
                return Task.FromResult(new TryResultOrError<CommandDescriptor>(new Error(Errors.UnsupportedCommand, "The command id is not valid. id={0}", id)));
            }
            else
            {
                return Task.FromResult(new TryResultOrError<CommandDescriptor>(command));
            }
        }

        /// <inheritdoc/>
        public Task<TryResultOrError<CommandDescriptor>> TryFindByNameAsync(string name)
        {
            var command = commandIdentities.FirstOrDefault(e => name.Equals(e.Name));
            if (command == null)
            {
                return Task.FromResult(new TryResultOrError<CommandDescriptor>(new Error(Errors.UnsupportedCommand, "The command name is not valid. name={0}", name)));
            }
            else
            {
                return Task.FromResult(new TryResultOrError<CommandDescriptor>(command));
            }
        }

        /// <inheritdoc/>
        public Task<TryResultOrError<CommandDescriptor>> TryFindByPrefixAsync(string prefix)
        {
            var command = commandIdentities.FirstOrDefault(e => prefix.Equals(e.Prefix));
            if (command == null)
            {
                return Task.FromResult(new TryResultOrError<CommandDescriptor>(new Error(Errors.UnsupportedCommand, "The command prefix is not valid. prefix={0}", prefix)));
            }
            else
            {
                return Task.FromResult(new TryResultOrError<CommandDescriptor>(command));
            }
        }

        private readonly IEnumerable<CommandDescriptor> commandIdentities;
    }
}
