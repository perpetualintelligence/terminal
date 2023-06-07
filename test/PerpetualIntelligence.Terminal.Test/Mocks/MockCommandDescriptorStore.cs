/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Stores;
using PerpetualIntelligence.Shared.Infrastructure;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Mocks
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
