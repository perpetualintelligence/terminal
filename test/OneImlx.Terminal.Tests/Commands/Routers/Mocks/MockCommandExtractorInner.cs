/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Parsers;

namespace OneImlx.Terminal.Commands.Routers.Mocks
{
    internal class MockCommandParserInner : ICommandParser
    {
        public bool Called { get; set; }

        public bool DoNotSetCommandDescriptor { get; set; }

        public bool DoNotSetParsedCommand { get; set; }

        public CommandContext? PassedContext { get; internal set; }

        public bool SetExplicitError { get; set; }

        public Task ParseCommandAsync(CommandContext context)
        {
            Called = true;
            PassedContext = context;

            if (SetExplicitError)
            {
                throw new TerminalException("test_parser_error", "test_parser_error_desc");
            }
            else
            {
                if (DoNotSetParsedCommand)
                {
                    context.ParsedCommand = null;
                }
                else if (DoNotSetCommandDescriptor)
                {
                    context.ParsedCommand = new ParsedCommand(new Command(null!), null);
                }
                else
                {
                    // all ok
                    context.ParsedCommand = new ParsedCommand(new Command(new CommandDescriptor("test_id", "test_name", "desc", CommandType.SubCommand, CommandFlags.None)), null);
                }
            }

            return Task.CompletedTask;
        }
    }
}
