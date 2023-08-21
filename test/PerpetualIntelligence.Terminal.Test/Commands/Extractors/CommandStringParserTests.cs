/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    public class CommandStringParserTests
    {
        private readonly ITextHandler _textHandlerMock;
        private readonly CommandDescriptors _commandDescriptors;
        private readonly TerminalOptions _terminalOptions;

        public CommandStringParserTests()
        {
            _textHandlerMock = new AsciiTextHandler();
            _commandDescriptors = new CommandDescriptors(_textHandlerMock, MockCommands.Commands);
            _terminalOptions = MockTerminalOptions.NewAliasOptions();
        }

        [Theory]
        [InlineData("root1 grp1 grp2 cmd1 val1 val2 \"string value within\" --opt1 v1 -o2 v2 --opt3 v3 --opt4 \"string option value within\" --opt7 -o5 sad -o6 34.56 -o8")]
        public async Task ParseAsync_Should_Parse_Simple_Command_Strings(string commandString)
        {
            var parser = new CommandStringParser(_textHandlerMock, _commandDescriptors, _terminalOptions);
            Root root = await parser.ParseAsync(new CommandRoute(Guid.NewGuid().ToString(), commandString));
            root.Should().NotBeNull();
        }
    }
}