/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Cli.Commands.Providers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockDefaultArgumentProvider : IDefaultOptionProvider
    {
        public bool Called { get; set; }

        public Task<DefaultOptionProviderResult> ProvideAsync(DefaultOptionProviderContext context)
        {
            Called = true;
            return Task.FromResult(new DefaultOptionProviderResult(new Commands.OptionDescriptor("testid", System.ComponentModel.DataAnnotations.DataType.Text, "desc")));
        }
    }
}
