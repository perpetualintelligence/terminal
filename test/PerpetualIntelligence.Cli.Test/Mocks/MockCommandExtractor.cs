/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
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

            var cIdt = new Commands.CommandDescriptor("testid", "testname", "testname");
            return Task.FromResult(new CommandExtractorResult(new Commands.Command(cIdt), cIdt));
        }
    }
}
