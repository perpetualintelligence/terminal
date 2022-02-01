/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Providers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockDefaultArgumentProvider : IDefaultArgumentProvider
    {
        public bool Called { get; set; }

        public Task<DefaultArgumentProviderResult> ProvideAsync(DefaultArgumentProviderContext context)
        {
            Called = true;
            return Task.FromResult(new DefaultArgumentProviderResult(new Commands.ArgumentDescriptor("testid", System.ComponentModel.DataAnnotations.DataType.Text)));
        }
    }
}
