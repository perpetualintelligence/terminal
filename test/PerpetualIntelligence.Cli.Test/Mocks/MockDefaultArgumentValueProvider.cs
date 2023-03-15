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
    public class MockDefaultArgumentValueProvider : IDefaultOptionValueProvider
    {
        public bool Called { get; set; }

        public Task<DefaultOptionValueProviderResult> ProvideAsync(DefaultOptionValueProviderContext context)
        {
            Called = true;
            return Task.FromResult(new DefaultOptionValueProviderResult(new Commands.OptionDescriptors(new UnicodeTextHandler())));
        }
    }
}
