/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Stores;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Stores.InMemory
{
    /// <summary>
    /// The default in-memory <see cref="ICommandIdentityStore"/>.
    /// </summary>
    public class InMemoryCommandIdentityStore : ICommandIdentityStore
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="commandIdentities">The command identities.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public InMemoryCommandIdentityStore(IEnumerable<CommandIdentity> commandIdentities, CliOptions options, ILogger<InMemoryCommandIdentityStore> logger)
        {
            this.commandIdentities = commandIdentities ?? throw new ArgumentNullException(nameof(commandIdentities));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public Task<TryResult<CommandIdentity>> TryFindByIdAsync(string id)
        {
            TryResult<CommandIdentity> result = new();

            var command = commandIdentities.FirstOrDefault(e => id.Equals(e.Id));
            if (command == null)
            {
                result.SetError(Errors.UnsupportedCommand, logger.FormatAndLog(LogLevel.Error, options.Logging, "The command id is not valid. id={0}", id));
            }
            else
            {
                // Why identity just return the command?
                result.Result = command;
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<TryResult<CommandIdentity>> TryFindByNameAsync(string name)
        {
            TryResult<CommandIdentity> result = new();

            var command = commandIdentities.FirstOrDefault(e => name.Equals(e.Name));
            if (command == null)
            {
                result.SetError(Errors.UnsupportedCommand, logger.FormatAndLog(LogLevel.Error, options.Logging, "The command name is not valid. name={0}", name));
            }
            else
            {
                // Why identity just return the command?
                result.Result = command;
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<TryResult<CommandIdentity>> TryFindByPrefixAsync(string prefix)
        {
            TryResult<CommandIdentity> result = new();

            var command = commandIdentities.FirstOrDefault(e => prefix.Equals(e.Prefix));
            if (command == null)
            {
                result.SetError(Errors.UnsupportedCommand, logger.FormatAndLog(LogLevel.Error, options.Logging, "The command prefix is not valid. prefix={0}", prefix));
            }
            else
            {
                // Why identity just return the command?
                result.Result = command;
            }

            return Task.FromResult(result);
        }

        private readonly IEnumerable<CommandIdentity> commandIdentities;
        private readonly ILogger<InMemoryCommandIdentityStore> logger;
        private readonly CliOptions options;
    }
}
