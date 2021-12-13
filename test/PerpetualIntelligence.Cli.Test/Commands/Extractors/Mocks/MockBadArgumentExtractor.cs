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
            // No error no argument
            return Task.FromResult(new ArgumentExtractorResult { Argument = null });
        }
    }
}
