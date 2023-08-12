/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Shared.Infrastructure;
using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Stores.InMemory
{
    /// <summary>
    /// The default in-memory <see cref="ICommandStoreHandler"/>.
    /// </summary>
    public class InMemoryCommandStore : ICommandStoreHandler
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="commandDescriptors">The command identities.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public InMemoryCommandStore(ITextHandler textHandler, IEnumerable<CommandDescriptor> commandDescriptors, TerminalOptions options, ILogger<InMemoryCommandStore> logger)
        {
            this.textHandler = textHandler;
            this.commandDescriptors = commandDescriptors ?? throw new ArgumentNullException(nameof(commandDescriptors));
            this.options = options;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public Task<TryResultOrError<CommandDescriptor>> TryFindByIdAsync(string id)
        {
            var command = commandDescriptors.FirstOrDefault(e => textHandler.TextEquals(id, e.Id));
            if (command == null)
            {
                return Task.FromResult(new TryResultOrError<CommandDescriptor>(new Error(TerminalErrors.UnsupportedCommand, "The command id is not valid. id={0}", id)));
            }
            else
            {
                return Task.FromResult(new TryResultOrError<CommandDescriptor>(command));
            }
        }

        /// <inheritdoc/>
        public Task<TryResultOrError<CommandDescriptor>> TryFindByNameAsync(string name)
        {
            var command = commandDescriptors.FirstOrDefault(e => textHandler.TextEquals(name, e.Name));
            if (command == null)
            {
                return Task.FromResult(new TryResultOrError<CommandDescriptor>(new Error(TerminalErrors.UnsupportedCommand, "The command name is not valid. name={0}", name)));
            }
            else
            {
                return Task.FromResult(new TryResultOrError<CommandDescriptor>(command));
            }
        }

        /// <inheritdoc/>
        public Task<TryResultOrError<CommandDescriptor>> TryFindByPrefixAsync(string prefix)
        {
            var command = commandDescriptors.FirstOrDefault(e => textHandler.TextEquals(prefix, e.Prefix));
            if (command == null)
            {
                return Task.FromResult(new TryResultOrError<CommandDescriptor>(new Error(TerminalErrors.UnsupportedCommand, "The command prefix is not valid. prefix={0}", prefix)));
            }
            else
            {
                return Task.FromResult(new TryResultOrError<CommandDescriptor>(command));
            }
        }

        /// <inheritdoc/>
        public Task<TryResultOrError<CommandDescriptor>> TryMatchByPrefixAsync(string prefix)
        {
            Dictionary<CommandDescriptor, int> matches = new();
            foreach (var cmd in commandDescriptors)
            {
                string regEx = $"^{cmd.Prefix}$";
                Match match = Regex.Match(prefix, regEx);
                if (match.Success)
                {
                    return Task.FromResult(new TryResultOrError<CommandDescriptor>(cmd));
                }
            }

            // To get the closet match we take the command that has the most chars matched. Match.Length is the number
            // of chars that are captured during RegEx.Match. Order all the command in descending order and then get the
            // first matched command to get the closet match
            if (matches.Count > 0)
            {
                CommandDescriptor closestMatch = matches.OrderByDescending(e => e.Value).First().Key;
                return Task.FromResult(new TryResultOrError<CommandDescriptor>(closestMatch));
            }
            else
            {
                // Error, we did not find a match
                return Task.FromResult(new TryResultOrError<CommandDescriptor>(new Error(TerminalErrors.UnsupportedCommand, "The command prefix is not valid. prefix={0}", prefix)));
            }
        }

        private readonly IEnumerable<CommandDescriptor> commandDescriptors;
        private readonly ILogger<InMemoryCommandStore> logger;
        private readonly TerminalOptions options;
        private readonly ITextHandler textHandler;
    }
}