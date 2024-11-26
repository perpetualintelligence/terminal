/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Routers.Mocks
{
    internal class MockCommandParserInner : ICommandParser
    {
        public bool Called { get; set; }

        public bool IsExplicitError { get; set; }

        public bool IsExplicitNoExtractedCommand { get; set; }

        public bool IsExplicitNoCommandDescriptor { get; set; }

        public Task<CommandParserResult> ParseCommandAsync(CommandParserContext context)
        {
            Called = true;

            if (IsExplicitError)
            {
                throw new TerminalException("test_parser_error", "test_parser_error_desc");
            }
            else
            {
                if (IsExplicitNoExtractedCommand)
                {
                    // No error but no command
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    return Task.FromResult(new CommandParserResult(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                }
                else if (IsExplicitNoCommandDescriptor)
                {
                    // No error but no command descriptor
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    return Task.FromResult(new CommandParserResult(new ParsedCommand(new TerminalRequest("id1", "test1"), new Command(null), Root.Default())));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                }
                else
                {
                    // all ok
                    return Task.FromResult(new CommandParserResult(new ParsedCommand(new TerminalRequest("id1", "test1"), new Command(new CommandDescriptor("test_id", "test_name", "desc", CommandType.SubCommand, CommandFlags.None)), Root.Default())));
                }
            }
        }
    }
}