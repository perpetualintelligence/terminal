/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Terminal.Stores;
using PerpetualIntelligence.Terminal.Stores.InMemory;
using PerpetualIntelligence.Test.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    public class UnicodeHindiCommandParserTests
    {
        private ICommandRouteParser routeParser;
        private ICommandStoreHandler commandStore;
        private TerminalOptions options;
        private ITextHandler textHandler;

        public UnicodeHindiCommandParserTests()
        {
            options = MockTerminalOptions.NewAliasOptions();
            textHandler = new UnicodeTextHandler();
            commandStore = new InMemoryCommandStore(MockCommands.UnicodeCommands);
            routeParser = new CommandRouteParser(textHandler, commandStore, options, TestLogger.Create<CommandRouteParser>());
        }

        [Fact]
        public async Task UnicodeGroupedCommand_Parses_Correctly()
        {
            CommandRoute commandRoute = new("id1", "यूनिकोड परीक्षण");
            ParsedCommand result = await routeParser.ParseAsync(commandRoute);

            result.Should().NotBeNull();
            result.Command.Id.Should().Be("परीक्षण");
            result.Command.Name.Should().Be("परीक्षण नाम");
            result.Command.Description.Should().Be("यूनिकोड समूहीकृत कमांड");
            result.Command.Options.Should().BeNull();

            result.Command.Descriptor.Should().NotBeNull();
            result.Command.Descriptor.Type.Should().Be(CommandType.Group);

            result.Command.Options.Should().BeNull();
            result.Command.Arguments.Should().BeNull();
        }

        [Fact]
        public async Task UnicodeGroupedCommand_With_Incomplete_Prefix_ShouldError()
        {
            CommandRoute commandRoute = new("id1", "परीक्षण");
            await TestHelper.AssertThrowsErrorExceptionAsync(() => routeParser.ParseAsync(commandRoute), TerminalErrors.UnsupportedCommand, "The command prefix is not valid. prefix=परीक्षण");
        }

        [Fact]
        public async Task UnicodeRootCommand_Parses_Correctly()
        {
            CommandRoute commandRoute = new("id1", "यूनिकोड");
            ParsedCommand result = await routeParser.ParseAsync(commandRoute);

            result.Should().NotBeNull();
            result.Command.Id.Should().Be("यूनिकोड");
            result.Command.Name.Should().Be("यूनिकोड नाम");
            result.Command.Description.Should().Be("यूनिकोड रूट कमांड");
            result.Command.Options.Should().BeNull();

            result.Command.Descriptor.Should().NotBeNull();
            result.Command.Descriptor.Type.Should().Be(CommandType.Root);

            result.Command.Options.Should().BeNull();
            result.Command.Arguments.Should().BeNull();
        }

        [Fact]
        public async Task UnicodeSubCommand_Parses_Correctly()
        {
            CommandRoute commandRoute = new("id1", "यूनिकोड परीक्षण प्रिंट");
            var result = await routeParser.ParseAsync(commandRoute);

            result.Should().NotBeNull();
            result.Command.Id.Should().Be("प्रिंट");
            result.Command.Name.Should().Be("प्रिंट नाम");
            result.Command.Description.Should().Be("प्रिंट कमांड");

            result.Command.Descriptor.Should().NotBeNull();
            result.Command.Descriptor.Type.Should().Be(CommandType.SubCommand);

            result.Command.Options.Should().BeNull();
            result.Command.Arguments.Should().BeNull();
        }

        [Fact]
        public async Task UnicodeSubCommand_With_Options_Parses_Correctly()
        {
            CommandRoute commandRoute = new("id1", "यूनिकोड परीक्षण प्रिंट --एक पहला मूल्य --दो --तीन तीसरा मूल्य --चार 253.36");
            var result = await routeParser.ParseAsync(commandRoute);

            result.Should().NotBeNull();
            result.Command.Id.Should().Be("प्रिंट");
            result.Command.Name.Should().Be("प्रिंट नाम");
            result.Command.Description.Should().Be("प्रिंट कमांड");

            result.Command.Descriptor.Should().NotBeNull();
            result.Command.Descriptor.Type.Should().Be(CommandType.SubCommand);

            result.Command.Options.Should().HaveCount(4);
            AssertOption(result.Command.Options!["एक"], "एक", nameof(String), "पहला तर्क", "पहला मूल्य");
            AssertOption(result.Command.Options["दो"], "दो", nameof(Boolean), "दूसरा तर्क", true.ToString());
            AssertOption(result.Command.Options["तीन"], "तीन", nameof(String), "तीसरा तर्क", "तीसरा मूल्य");
            AssertOption(result.Command.Options["चार"], "चार", nameof(Double), "चौथा तर्क", "253.36");
        }

        [Fact]
        public async Task UnicodeSubCommand_Alias_Should_Parses_Correctly()
        {
            // एकहै and चारहै are alias
            CommandRoute commandRoute = new("id1", "यूनिकोड परीक्षण प्रिंट -एकहै पहला मूल्य --दो --तीन तीसरा मूल्य -चारहै 253.36");
            var result = await routeParser.ParseAsync(commandRoute);

            result.Command.Descriptor.Should().NotBeNull();

            result.Should().NotBeNull();
            result.Command.Id.Should().Be("uc3");
            result.Command.Name.Should().Be("प्रिंट");
            result.Command.Description.Should().Be("प्रिंट कमांड");
            result.Command.Options.Should().HaveCount(4);

            AssertOption(result.Command.Options!["एक"], "एक", nameof(String), "पहला तर्क", "पहला मूल्य");
            AssertOption(result.Command.Options["दो"], "दो", nameof(Boolean), "दूसरा तर्क", true.ToString());
        }

        private void AssertOption(Option arg, string name, string dataType, string description, object value)
        {
            arg.Should().NotBeNull();
            arg.Id.Should().Be(name);
            arg.DataType.Should().Be(dataType);
            arg.Description.Should().Be(description);
            arg.Value.Should().Be(value);
        }
    }
}