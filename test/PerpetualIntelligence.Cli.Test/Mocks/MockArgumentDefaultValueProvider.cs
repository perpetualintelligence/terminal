/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Providers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockArgumentDefaultValueProvider : IArgumentDefaultValueProvider
    {
        public bool Called { get; set; }

        public Task<ArgumentDefaultValueProviderResult> ProvideAsync(ArgumentDefaultValueProviderContext context)
        {
            Called = true;
            return Task.FromResult(new ArgumentDefaultValueProviderResult(new Commands.ArgumentDescriptors()));
        }
    }
}
