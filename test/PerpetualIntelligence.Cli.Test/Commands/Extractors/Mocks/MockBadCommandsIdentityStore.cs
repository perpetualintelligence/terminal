/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Stores;
using PerpetualIntelligence.Shared.Infrastructure;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Extractors.Mocks
{
    internal class MockBadCommandsIdentityStore : ICommandIdentityStore
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
            // No error and no result
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            return Task.FromResult(new OneImlxTryResult<CommandIdentity>(result: null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }
    }
}
