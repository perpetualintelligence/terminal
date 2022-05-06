/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Providers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockDefaultArgumentValueProvider : IDefaultArgumentValueProvider
    {
        public bool Called { get; set; }

        public Task<DefaultArgumentValueProviderResult> ProvideAsync(DefaultArgumentValueProviderContext context)
        {
            Called = true;
            return Task.FromResult(new DefaultArgumentValueProviderResult(new Commands.ArgumentDescriptors(new UnicodeTextHandler())));
        }
    }
}
