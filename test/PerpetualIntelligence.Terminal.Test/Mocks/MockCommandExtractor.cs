/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Commands.Extractors;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Mocks
{
    public class MockCommandExtractor : ICommandExtractor
    {
        public bool Called { get; set; }

        public Task<CommandExtractorResult> ExtractAsync(CommandExtractorContext context)
        {
            Called = true;

            var cIdt = new Commands.CommandDescriptor("testid", "testname", "testname", "desc");
            Command command = new(cIdt);
            ExtractedCommand extractedCommand = new(new CommandRoute("id1", "test"), command, Root.Default());
            return Task.FromResult(new CommandExtractorResult(extractedCommand));
        }
    }
}