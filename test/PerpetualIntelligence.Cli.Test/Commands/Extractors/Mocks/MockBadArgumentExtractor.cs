/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Extractors.Mocks
{
    internal class MockBadArgumentExtractor : IArgumentExtractor
    {
        public Task<ArgumentExtractorResult> ExtractAsync(ArgumentExtractorContext context)
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            return Task.FromResult(new ArgumentExtractorResult(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }
    }
}
