/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
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
