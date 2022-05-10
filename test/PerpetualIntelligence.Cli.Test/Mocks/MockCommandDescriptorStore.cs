/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Stores;
using PerpetualIntelligence.Shared.Infrastructure;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockCommandDescriptorStore : ICommandStoreHandler
    {
        public Task<TryResultOrError<CommandDescriptor>> TryFindByIdAsync(string id)
        {
            throw new System.NotImplementedException();
        }

        public Task<TryResultOrError<CommandDescriptor>> TryFindByNameAsync(string name)
        {
            throw new System.NotImplementedException();
        }

        public Task<TryResultOrError<CommandDescriptor>> TryFindByPrefixAsync(string prefix)
        {
            throw new System.NotImplementedException();
        }

        public Task<TryResultOrError<CommandDescriptor>> TryMatchByPrefixAsync(string prefix)
        {
            throw new System.NotImplementedException();
        }
    }
}
