/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Extractors.Mocks
{
    internal class MockBadOptionExtractor : IOptionExtractor
    {
        public Task<OptionExtractorResult> ExtractAsync(OptionExtractorContext context)
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            return Task.FromResult(new OptionExtractorResult(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }
    }
}
