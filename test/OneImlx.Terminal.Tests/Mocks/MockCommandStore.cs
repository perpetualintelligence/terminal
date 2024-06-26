﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Stores;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Mocks
{
    public class MockCommandStore : ITerminalCommandStore
    {
        public Task<CommandDescriptors> AllAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> TryAddAsync(string id, CommandDescriptor commandDescriptor)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> TryFindByIdAsync(string id, out CommandDescriptor? commandDescriptor)
        {
            throw new System.NotImplementedException();
        }
    }
}