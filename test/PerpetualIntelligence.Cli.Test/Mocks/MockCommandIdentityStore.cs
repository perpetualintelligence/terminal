/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Stores;
using PerpetualIntelligence.Shared.Infrastructure;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockCommandIdentityStore : ICommandIdentityStore
    {
        public Task<TryResult<CommandIdentity>> TryFindByIdAsync(string id)
        {
            throw new System.NotImplementedException();
        }

        public Task<TryResult<CommandIdentity>> TryFindByNameAsync(string name)
        {
            throw new System.NotImplementedException();
        }

        public Task<TryResult<CommandIdentity>> TryFindByPrefixAsync(string prefix)
        {
            throw new System.NotImplementedException();
        }

        public Task<TryResult<CommandIdentity>> TryMatchByPrefixAsync(string commandString)
        {
            throw new System.NotImplementedException();
        }
    }
}
