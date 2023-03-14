/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Extractors.Mocks
{
    internal class MockBadArgumentExtractor : IOptionExtractor
    {
        public Task<OptionExtractorResult> ExtractAsync(OptionExtractorContext context)
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            return Task.FromResult(new OptionExtractorResult(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }
    }
}
