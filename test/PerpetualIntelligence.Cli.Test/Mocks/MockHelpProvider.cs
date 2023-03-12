/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Providers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
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