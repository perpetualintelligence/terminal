/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Parsers;

namespace OneImlx.Terminal.Mocks
{
    public class MockCommandParser : ICommandParser
    {
        public Task ParseCommandAsync(CommandContext context)
        {
            Called = true;

            var cIdt = new CommandDescriptor("testid", "testname", "desc", CommandType.SubCommand, CommandFlags.None);
            Command command = new(cIdt);
            ParsedCommand extractedCommand = new(command, null);
            context.ParsedCommand = extractedCommand;
            return Task.CompletedTask;
        }

        public bool Called { get; set; }
    }
}
