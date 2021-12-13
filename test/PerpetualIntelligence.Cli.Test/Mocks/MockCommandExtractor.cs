/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Extractors;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockCommandExtractor : ICommandExtractor
    {
        public bool Called { get; set; }

        public Task<CommandExtractorResult> ExtractAsync(CommandExtractorContext context)
        {
            Called = true;
            return Task.FromResult(new CommandExtractorResult());
        }
    }
}
