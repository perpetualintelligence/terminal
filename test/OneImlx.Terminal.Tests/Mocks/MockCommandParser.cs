/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Parsers;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Mocks
{
    public class MockCommandParser : ICommandParser
    {
        public bool Called { get; set; }

        public Task<CommandParserResult> ParseCommandAsync(CommandParserContext context)
        {
            Called = true;

            var cIdt = new Commands.CommandDescriptor("testid", "testname", "desc", CommandType.SubCommand, CommandFlags.None);
            Command command = new(cIdt);
            ParsedCommand extractedCommand = new(new CommandRoute("id1", "test"), command, Root.Default());
            return Task.FromResult(new CommandParserResult(extractedCommand));
        }
    }
}