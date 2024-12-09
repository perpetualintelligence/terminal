/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Stores;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Mocks
{
    public class MockCommandStore : ITerminalCommandStore
    {
        public MockCommandStore()
        {
        }

        public MockCommandStore(CommandDescriptors commandDescriptors)
        {
            this.commandDescriptors = commandDescriptors;
        }

        public Task<CommandDescriptors> AllAsync()
        {
            return Task.FromResult(commandDescriptors);
        }

        public Task<bool> TryAddAsync(string id, CommandDescriptor commandDescriptor)
        {
            commandDescriptors.Add(id, commandDescriptor);
            return Task.FromResult(true);
        }

        public Task<bool> TryFindByIdAsync(string id, out CommandDescriptor? commandDescriptor)
        {
            return Task.FromResult(commandDescriptors.TryGetValue(id, out commandDescriptor));
        }

        private readonly CommandDescriptors commandDescriptors;
    }
}
