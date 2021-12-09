/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Shared.Infrastructure;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockCommandIdentityStore : ICommandIdentityStore
    {
        public Task<OneImlxTryResult<CommandIdentity>> TryFindByIdAsync(string id)
        {
            throw new System.NotImplementedException();
        }

        public Task<OneImlxTryResult<CommandIdentity>> TryFindByNameAsync(string name)
        {
            throw new System.NotImplementedException();
        }

        public Task<OneImlxTryResult<CommandIdentity>> TryFindByPrefixAsync(string prefix)
        {
            throw new System.NotImplementedException();
        }

        public Task<OneImlxTryResult<CommandIdentity>> TryMatchByPrefixAsync(string commandString)
        {
            throw new System.NotImplementedException();
        }
    }
}
