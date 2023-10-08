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

        public Task<CommandExtractorResult> ExtractCommandAsync(CommandExtractorContext context)
        {
            Called = true;

            var cIdt = new Commands.CommandDescriptor("testid", "testname", "desc", CommandType.SubCommand, CommandFlags.None);
            Command command = new(cIdt);
            ParsedCommand extractedCommand = new(new CommandRoute("id1", "test"), command, Root.Default());
            return Task.FromResult(new CommandExtractorResult(extractedCommand));
        }
    }
}