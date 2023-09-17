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
            _textHandler = new UnicodeTextHandler();
            _commandStore = new InMemoryCommandStore(MockCommands.UnicodeCommands);
            _terminalOptions = MockTerminalOptions.NewAliasOptions();
            _commandRouteParser = new CommandRouteParser(_textHandler, _commandStore, _terminalOptions, _logger);
        }

        [Fact]
        public async Task Unicode_Hindi_Root_Extracts_Correctly()
        {
            var parsedCommand = await _commandRouteParser.ParseAsync(new CommandRoute("यूनिकोड", "यूनिकोड"));

            parsedCommand.Command.Descriptor.Should().NotBeNull();
            parsedCommand.Command.Descriptor.Type.Should().Be(CommandType.Root);

            parsedCommand.Should().NotBeNull();
            parsedCommand.Command.Id.Should().Be("यूनिकोड");
            parsedCommand.Command.Name.Should().Be("यूनिकोड नाम");
            parsedCommand.Command.Description.Should().Be("यूनिकोड रूट कमांड");
            parsedCommand.Command.Options.Should().BeNull();
        }

        [Fact]
        public async Task Unicode_Hindi_Group_Extracts_Correctly()
        {
            var parsedCommand = await _commandRouteParser.ParseAsync(new CommandRoute("परीक्षण", "यूनिकोड परीक्षण"));

            parsedCommand.Command.Descriptor.Should().NotBeNull();
            parsedCommand.Command.Descriptor.Type.Should().Be(CommandType.Group);

            parsedCommand.Should().NotBeNull();
            parsedCommand.Command.Id.Should().Be("परीक्षण");
            parsedCommand.Command.Name.Should().Be("परीक्षण नाम");
            parsedCommand.Command.Description.Should().Be("यूनिकोड समूहीकृत कमांड");
            parsedCommand.Command.Options.Should().BeNull();

            parsedCommand.Hierarchy.LinkedCommand.Id.Should().Be("यूनिकोड");
            parsedCommand.Hierarchy.ChildSubCommand.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup!.LinkedCommand.Id.Should().Be("परीक्षण");
            parsedCommand.Hierarchy.ChildGroup.ChildGroup.Should().BeNull();
            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand.Should().BeNull();
        }

        [Fact]
        public async Task Unicode_Hindi_SubCommand_Extracts_Correctly()
        {
            var parsedCommand = await _commandRouteParser.ParseAsync(new CommandRoute("प्रिंट", "यूनिकोड परीक्षण प्रिंट"));

            parsedCommand.Command.Descriptor.Should().NotBeNull();
            parsedCommand.Command.Descriptor.Type.Should().Be(CommandType.SubCommand);

            parsedCommand.Should().NotBeNull();
            parsedCommand.Command.Id.Should().Be("प्रिंट");
            parsedCommand.Command.Name.Should().Be("प्रिंट नाम");
            parsedCommand.Command.Description.Should().Be("प्रिंट कमांड");
            parsedCommand.Command.Options.Should().BeNull();

            parsedCommand.Hierarchy.LinkedCommand.Id.Should().Be("यूनिकोड");
            parsedCommand.Hierarchy.ChildSubCommand.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup!.LinkedCommand.Id.Should().Be("परीक्षण");
            parsedCommand.Hierarchy.ChildGroup.ChildGroup.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand!.LinkedCommand.Id.Should().Be("प्रिंट");
        }

        [Fact]
        public async Task Unicode_Hindi_SubCommand_Second_Extracts_Correctly()
        {
            var parsedCommand = await _commandRouteParser.ParseAsync(new CommandRoute("दूसरा", "यूनिकोड परीक्षण दूसरा"));

            parsedCommand.Command.Descriptor.Should().NotBeNull();
            parsedCommand.Command.Descriptor.Type.Should().Be(CommandType.SubCommand);

            parsedCommand.Should().NotBeNull();
            parsedCommand.Command.Id.Should().Be("दूसरा");
            parsedCommand.Command.Name.Should().Be("दूसरा नाम");
            parsedCommand.Command.Description.Should().Be("दूसरा आदेश");
            parsedCommand.Command.Options.Should().BeNull();

            parsedCommand.Hierarchy.LinkedCommand.Id.Should().Be("यूनिकोड");
            parsedCommand.Hierarchy.ChildSubCommand.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup!.LinkedCommand.Id.Should().Be("परीक्षण");
            parsedCommand.Hierarchy.ChildGroup.ChildGroup.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand!.LinkedCommand.Id.Should().Be("दूसरा");
        }

        [Fact]
        public async Task Complex_Extracts()
        {
            string cmdRoute = "root1 grp1 grp2 cmd1 val1 val2 \"\"string value within\"\" --opt1 v1 -o2 v2 --opt3 v3 --opt4 string option value within --opt7 -o5 sad -o6 34.56 -o8 --opt9 \"\"string option value within\"\" --opt10 value without delimiter --opt11";
            var parsedCommand = await _commandRouteParser.ParseAsync(new CommandRoute("id1", cmdRoute));
        }

        [Fact]
        public async Task Unicode_Hindi_SubCommand_Options_Extracts_Correctly()
        {
            var parsedCommand = await _commandRouteParser.ParseAsync(new CommandRoute("प्रिंट", "यूनिकोड परीक्षण प्रिंट --एक \"प्रथम मान\" --दो -तीनहै \"तीसरा मान\" --चार 86.39"));

            parsedCommand.Command.Descriptor.Should().NotBeNull();
            parsedCommand.Command.Descriptor.Type.Should().Be(CommandType.SubCommand);

            parsedCommand.Should().NotBeNull();
            parsedCommand.Command.Id.Should().Be("प्रिंट");
            parsedCommand.Command.Name.Should().Be("प्रिंट नाम");
            parsedCommand.Command.Description.Should().Be("प्रिंट कमांड");
            parsedCommand.Command.Options.Should().BeNull();

            parsedCommand.Hierarchy.LinkedCommand.Id.Should().Be("यूनिकोड");
            parsedCommand.Hierarchy.ChildSubCommand.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup!.LinkedCommand.Id.Should().Be("परीक्षण");
            parsedCommand.Hierarchy.ChildGroup.ChildGroup.Should().BeNull();

            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand.Should().NotBeNull();
            parsedCommand.Hierarchy.ChildGroup.ChildSubCommand!.LinkedCommand.Id.Should().Be("प्रिंट");
        }
    }
}