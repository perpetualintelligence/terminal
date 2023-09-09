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
    public class UnicodeHindiCommandExtractorTests
    {
        private ICommandRouteParser routeParser;
        private ICommandStoreHandler commandStore;
        private CommandExtractor extractor;
        private TerminalOptions options;
        private ITextHandler textHandler;

        public UnicodeHindiCommandExtractorTests()
        {
            options = MockTerminalOptions.NewAliasOptions();
            textHandler = new UnicodeTextHandler();
            routeParser = new MockCommandRouteParser();
            commandStore = new InMemoryCommandStore(MockCommands.UnicodeCommands);
            extractor = new CommandExtractor(routeParser);
        }

        [Fact]
        public async Task UnicodeGroupedCommand_Should_Extract_Correctly()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "यूनिकोड परीक्षण"));
            var result = await extractor.ExtractAsync(context);

            result.ParsedCommand.Command.Descriptor.Should().NotBeNull();
            result.ParsedCommand.Command.Descriptor.Type.Should().Be(CommandType.Group);

            result.ParsedCommand.Should().NotBeNull();
            result.ParsedCommand.Command.Id.Should().Be("uc2");
            result.ParsedCommand.Command.Name.Should().Be("परीक्षण");
            result.ParsedCommand.Command.Description.Should().Be("यूनिकोड समूहीकृत कमांड");
            result.ParsedCommand.Command.Options.Should().BeNull();
        }

        [Fact]
        public async Task UnicodeGroupedCommand_With_Incomplete_Prefix_ShouldError()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "परीक्षण"));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), TerminalErrors.UnsupportedCommand, "The command prefix is not valid. prefix=परीक्षण");
        }

        [Fact]
        public async Task UnicodeRootCommand_Should_Extract_Correctly()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "यूनिकोड"));
            var result = await extractor.ExtractAsync(context);

            result.ParsedCommand.Command.Descriptor.Should().NotBeNull();
            result.ParsedCommand.Command.Descriptor.Type.Should().Be(CommandType.Root);

            result.ParsedCommand.Should().NotBeNull();
            result.ParsedCommand.Command.Id.Should().Be("uc1");
            result.ParsedCommand.Command.Name.Should().Be("यूनिकोड");
            result.ParsedCommand.Command.Description.Should().Be("यूनिकोड रूट कमांड");
            result.ParsedCommand.Command.Options.Should().BeNull();
        }

        [Fact]
        public async Task UnicodeSubCommand_Should_Extract_Correctly()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "यूनिकोड परीक्षण प्रिंट --एक पहला मूल्य --दो --तीन तीसरा मूल्य --चार 253.36"));
            var result = await extractor.ExtractAsync(context);

            result.ParsedCommand.Command.Descriptor.Should().NotBeNull();

            result.ParsedCommand.Should().NotBeNull();
            result.ParsedCommand.Command.Id.Should().Be("uc3");
            result.ParsedCommand.Command.Name.Should().Be("प्रिंट");
            result.ParsedCommand.Command.Description.Should().Be("प्रिंट कमांड");
            result.ParsedCommand.Command.Options.Should().HaveCount(4);

            AssertOption(result.ParsedCommand.Command.Options![0], "एक", nameof(String), "पहला तर्क", "पहला मूल्य");
            AssertOption(result.ParsedCommand.Command.Options[1], "दो", nameof(Boolean), "दूसरा तर्क", true.ToString());
            AssertOption(result.ParsedCommand.Command.Options[2], "तीन", nameof(String), "तीसरा तर्क", "तीसरा मूल्य");
            AssertOption(result.ParsedCommand.Command.Options[3], "चार", nameof(Double), "चौथा तर्क", "253.36");
        }

        [Fact]
        public async Task UnicodeSubCommand_Alias_Should_Extract_Correctly()
        {
            // एकहै and चारहै are alias
            CommandExtractorContext context = new(new CommandRoute("id1", "यूनिकोड परीक्षण प्रिंट -एकहै पहला मूल्य --दो --तीन तीसरा मूल्य -चारहै 253.36"));
            var result = await extractor.ExtractAsync(context);

            result.ParsedCommand.Command.Descriptor.Should().NotBeNull();

            result.ParsedCommand.Should().NotBeNull();
            result.ParsedCommand.Command.Id.Should().Be("uc3");
            result.ParsedCommand.Command.Name.Should().Be("प्रिंट");
            result.ParsedCommand.Command.Description.Should().Be("प्रिंट कमांड");
            result.ParsedCommand.Command.Options.Should().HaveCount(4);

            AssertOption(result.ParsedCommand.Command.Options![0], "एक", nameof(String), "पहला तर्क", "पहला मूल्य");
            AssertOption(result.ParsedCommand.Command.Options[1], "दो", nameof(Boolean), "दूसरा तर्क", true.ToString());
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