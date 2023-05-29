/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Commands.Providers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Mocks
{
    public class MockDefaultOptionValueProvider : IDefaultOptionValueProvider
    {
        public bool Called { get; set; }

        public Task<DefaultOptionValueProviderResult> ProvideAsync(DefaultOptionValueProviderContext context)
        {
            Called = true;
            return Task.FromResult(new DefaultOptionValueProviderResult(new Commands.OptionDescriptors(new UnicodeTextHandler())));
        }
    }
}
