/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Extractors;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockArgumentExtractor : IArgumentExtractor
    {
        public bool Called { get; set; }

        public Task<ArgumentExtractorResult> ExtractAsync(ArgumentExtractorContext context)
        {
            Called = true;
            return Task.FromResult(new ArgumentExtractorResult());
        }
    }
}
