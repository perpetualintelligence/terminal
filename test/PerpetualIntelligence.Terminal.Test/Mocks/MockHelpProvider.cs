/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Providers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Mocks
{
    internal class MockHelpProvider : IHelpProvider
    {
        public bool HelpCalled { get; private set; }

        public Task ProvideAsync(HelpProviderContext context)
        {
            HelpCalled = true;
            return Task.CompletedTask;
        }
    }
}