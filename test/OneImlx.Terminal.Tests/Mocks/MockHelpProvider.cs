/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands.Providers;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Mocks
{
    internal class MockHelpProvider : IHelpProvider
    {
        public bool HelpCalled { get; private set; }

        public Task ProvideHelpAsync(HelpProviderContext context)
        {
            HelpCalled = true;
            return Task.CompletedTask;
        }
    }
}