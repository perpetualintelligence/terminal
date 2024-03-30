/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Runtime;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Stores
{
    /// <summary>
    /// The default in-memory <see cref="ITerminalCommandStore"/>.
    /// </summary>
    public class TerminalInMemoryCommandStore : ITerminalCommandStore
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="textHandler"></param>
        /// <param name="commandDescriptors">The command identities.</param>
        public TerminalInMemoryCommandStore(ITerminalTextHandler textHandler, IEnumerable<CommandDescriptor> commandDescriptors)
        {
            this.commandDescriptors = new CommandDescriptors(textHandler, commandDescriptors);
        }

        /// <summary>
        /// Returns all command descriptors asynchronously.
        /// </summary>
        /// <returns>A <see cref="ReadOnlyDictionary{TKey, TValue}"/> of command descriptors.</returns>
        public Task<CommandDescriptors> AllAsync()
        {
            return Task.FromResult(commandDescriptors);
        }

        /// <inheritdoc/>
        public Task<bool> TryFindByIdAsync(string id, out CommandDescriptor? commandDescriptor)
        {
            return Task.FromResult(commandDescriptors.TryGetValue(id, out commandDescriptor));
        }

        /// <inheritdoc/>
        public Task<bool> TryAddAsync(string id, CommandDescriptor commandDescriptor)
        {
            try
            {
                commandDescriptors.Add(id, commandDescriptor);
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        private readonly CommandDescriptors commandDescriptors;
    }
}