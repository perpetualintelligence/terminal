/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Terminal.Stores;
using PerpetualIntelligence.Terminal.Stores.InMemory;
using PerpetualIntelligence.Test.Services;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    public class CommandRouteParserHindiTests
    {
        private readonly ITextHandler _textHandler;
        private readonly ICommandStoreHandler _commandStore;
        private readonly TerminalOptions _terminalOptions;
        private readonly ILogger<CommandRouteParser> _logger;
        private readonly ICommandRouteParser _commandRouteParser;

        public CommandRouteParserHindiTests()
        {
            _logger = TestLogger.Create<CommandRouteParser>();
            _textHandler = new AsciiTextHandler();
            _commandStore = new InMemoryCommandStore(MockCommands.UnicodeCommands);
            _terminalOptions = MockTerminalOptions.NewAliasOptions();
            _commandRouteParser = new CommandRouteParser(_textHandler, _commandStore, _terminalOptions, _logger);
        }

        [Theory]
        [InlineData("", "", "", "")]
        public async Task UnicodeCommand_Should_Extract_Correctly(string commandRoute, string id, string name, string description)
        {
            var parsedCommand = await _commandRouteParser.ParseAsync(new CommandRoute("", "यूनिकोड परीक्षण"));

            parsedCommand.Command.Descriptor.Should().NotBeNull();
            parsedCommand.Command.Descriptor.Type.Should().Be(CommandType.SubCommand);

            parsedCommand.Should().NotBeNull();
            parsedCommand.Command.Id.Should().Be("uc1");
            parsedCommand.Command.Name.Should().Be("परीक्षण");
            parsedCommand.Command.Description.Should().Be("यूनिकोड कमांड");
            parsedCommand.Command.Options.Should().BeNull();
        }

        [Fact]
        public async Task UnicodeGroupedCommand_Should_Extract_Correctly()
        {
            var parsedCommand = await _commandRouteParser.ParseAsync(new CommandRoute("uc1", "यूनिकोड परीक्षण"));

            parsedCommand.Command.Descriptor.Should().NotBeNull();
            parsedCommand.Command.Descriptor.Type.Should().Be(CommandType.Group);

            parsedCommand.Should().NotBeNull();
            parsedCommand.Command.Id.Should().Be("uc2");
            parsedCommand.Command.Name.Should().Be("परीक्षण");
            parsedCommand.Command.Description.Should().Be("यूनिकोड समूहीकृत कमांड");
            parsedCommand.Command.Options.Should().BeNull();
        }
    }
}