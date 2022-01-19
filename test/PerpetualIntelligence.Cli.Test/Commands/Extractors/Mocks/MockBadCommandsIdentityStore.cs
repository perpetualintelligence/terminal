﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Stores;
using PerpetualIntelligence.Shared.Infrastructure;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Extractors.Mocks
{
    internal class MockBadCommandsIdentityStore : ICommandIdentityStore
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
            // No error and no result
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            return Task.FromResult(new TryResult<CommandIdentity>(result: null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }
    }
}
