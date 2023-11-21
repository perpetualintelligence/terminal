/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Terminal.Stores;
using PerpetualIntelligence.Test.Services;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Parsers
{
    [TestClass]
    public class CommandParserTests
    {
        public CommandParserTests()
        {
            terminalOptions = MockTerminalOptions.NewLegacyOptions();
            textHandler = new UnicodeTextHandler();
            routeParser = new MockCommandRouteParser();
            commandStore = new InMemoryImmutableCommandStore(textHandler, MockCommands.Commands.Values);
            logger = TestLogger.Create<CommandParser>();
            parser = new CommandParser(routeParser, logger);
        }

        [TestMethod]
        public async Task Calls_Route_ParserAsync()
        {
            MockCommandRouteParser routeParser = new();
            CommandParser parser = new(routeParser, logger);

            CommandParserContext context = new(new CommandRoute("id1", "id1 test raw string"));
            await parser.ParseCommandAsync(context);

            routeParser.Called.Should().BeTrue();
            routeParser.PassedCommandRoute.Raw.Should().Be("id1 test raw string");
        }

        [TestMethod]
        public async Task UnspecifiedRequiredValuesShouldNotPopulateIfDisabled()
        {
            // This is just extracting no checking
            CommandParserContext context = new(new CommandRoute("id1", "prefix5_default"));
            var result = await parser.ParseCommandAsync(context);

            Assert.IsNull(result.ParsedCommand.Command.Options);
        }

        [TestMethod]
        public async Task ConfiguredCommandWithNoArgsShouldNotErrorAsync()
        {
            CommandParserContext context = new(new CommandRoute("id1", "prefix4_noargs"));
            var result = await parser.ParseCommandAsync(context);

            Assert.IsNull(result.ParsedCommand.Command.Options);
        }

        [TestMethod]
        public async Task DisabledButProviderNotConfiguredShouldNotThrow()
        {
            CommandParserContext context = new(new CommandRoute("id1", "prefix5_default"));
            CommandParser noProviderParser = new(routeParser, logger);
            await noProviderParser.ParseCommandAsync(context);
        }

        [TestMethod]
        public async Task CommandWithNoArgsShouldNotErrorAsync()
        {
            CommandParserContext context = new(new CommandRoute("id1", "prefix4_noargs"));
            var result = await parser.ParseCommandAsync(context);

            Assert.IsNull(result.ParsedCommand.Command.Options);
        }

        private IImmutableCommandStore commandStore = null!;
        private ICommandRouteParser routeParser = null!;
        private CommandParser parser = null!;
        private TerminalOptions terminalOptions = null!;
        private ITextHandler textHandler = null!;
        private ILogger<CommandParser> logger = null!;
    }
}