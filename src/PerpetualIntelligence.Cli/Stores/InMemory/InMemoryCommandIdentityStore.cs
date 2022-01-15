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
using PerpetualIntelligence.Shared.Attributes;
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
        public Task<OneImlxTryResult<CommandIdentity>> TryFindByIdAsync(string id)
        {
            OneImlxTryResult<CommandIdentity> result = new();

            var command = commandIdentities.FirstOrDefault(e => id.Equals(e.Id));
            if (command == null)
            {
                result.SetError(Errors.InvalidCommand, logger.FormatAndLog(LogLevel.Error, options.Logging, "The command id is not valid. id={0}", id));
            }
            else
            {
                // Why identity just return the command?
                result.Result = command;
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<OneImlxTryResult<CommandIdentity>> TryFindByNameAsync(string name)
        {
            OneImlxTryResult<CommandIdentity> result = new();

            var command = commandIdentities.FirstOrDefault(e => name.Equals(e.Name));
            if (command == null)
            {
                result.SetError(Errors.InvalidCommand, logger.FormatAndLog(LogLevel.Error, options.Logging, "The command name is not valid. name={0}", name));
            }
            else
            {
                // Why identity just return the command?
                result.Result = command;
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<OneImlxTryResult<CommandIdentity>> TryFindByPrefixAsync(string prefix)
        {
            OneImlxTryResult<CommandIdentity> result = new();

            var command = commandIdentities.FirstOrDefault(e => prefix.Equals(e.Prefix));
            if (command == null)
            {
                result.SetError(Errors.InvalidCommand, logger.FormatAndLog(LogLevel.Error, options.Logging, "The command prefix is not valid. prefix={0}", prefix));
            }
            else
            {
                // Why identity just return the command?
                result.Result = command;
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        [Performance("We are using distance algorithm to find the match by prefix.")]
        public Task<OneImlxTryResult<CommandIdentity>> TryMatchByPrefixAsync(string commandString)
        {
            // TODO: Improve performance. We don't stop at the first match; we continue to find a closet match because
            // the command group may match instead of the command itself. e.g., pi, pi auth, pi auth login, all will
            // match to pi command.

            // First find all the probable matches
            OneImlxTryResult<CommandIdentity> result = new();
            IEnumerable<CommandIdentity> matchedIdentities = commandIdentities.Where(e => commandString.StartsWith(e.Prefix, StringComparison.OrdinalIgnoreCase));
            if (matchedIdentities.Count() == 1)
            {
                // Just 1, thats the exact match
                result.Result = matchedIdentities.First();
                return Task.FromResult(result);
            }
            else
            {
                // Now find the closest probable match
                int matchScore = int.MaxValue;
                CommandIdentity? probableMatch = null;
                foreach (CommandIdentity command in matchedIdentities)
                {
                    // We want the closet match of the prefix so take the command that trims the max prefix chars
                    int remaining = commandString.TrimStart(command.Prefix).Length;
                    if (matchScore > remaining)
                    {
                        matchScore = remaining;
                        probableMatch = command;
                    }
                }

                // Most likely we found a match
                if (probableMatch != null)
                {
                    // We are trying to determine the most probable match, an invalid sub command will match the command
                    // group. E.g., 'pi auth cmd invalid' will match to 'pi auth cmd' command since that is closest match.
                    // There is no way for us to know if the command is invalid at this point, this will be handled as 
                    // an invalid argument.
                    result.Result = probableMatch;
                    return Task.FromResult(result);
                }

                result.SetError(Errors.InvalidCommand, logger.FormatAndLog(LogLevel.Error, options.Logging, "The command string did not match any command prefix. command_string={0}", commandString));
                return Task.FromResult(result);
            }
        }

        private readonly IEnumerable<CommandIdentity> commandIdentities;
        private readonly ILogger<InMemoryCommandIdentityStore> logger;
        private readonly CliOptions options;
    }
}
