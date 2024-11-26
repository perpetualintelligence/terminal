/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Commands.Parsers
{
    public class CommandParserTests
    {
        public CommandParserTests()
        {
            routeParser = new MockCommandRouteParser();
            logger = new LoggerFactory().CreateLogger<CommandParser>();
            parser = new CommandParser(routeParser, logger);
        }

        [Fact]
        public async Task Calls_Route_ParserAsync()
        {
            MockCommandRouteParser routeParser = new();
            CommandParser parser = new(routeParser, logger);

            CommandParserContext context = new(new TerminalRequest("id1", "id1 test raw string"));
            await parser.ParseCommandAsync(context);

            routeParser.Called.Should().BeTrue();
            routeParser.PassedCommandRoute.Raw.Should().Be("id1 test raw string");
        }

        [Fact]
        public async Task UnspecifiedRequiredValuesShouldNotPopulateIfDisabled()
        {
            // This is just extracting no checking
            CommandParserContext context = new(new TerminalRequest("id1", "prefix5_default"));
            var result = await parser.ParseCommandAsync(context);

            result.ParsedCommand.Command.Options.Should().BeNull();
        }

        [Fact]
        public async Task ConfiguredCommandWithNoArgsShouldNotErrorAsync()
        {
            CommandParserContext context = new(new TerminalRequest("id1", "prefix4_noargs"));
            var result = await parser.ParseCommandAsync(context);

            result.ParsedCommand.Command.Options.Should().BeNull();
        }

        [Fact]
        public async Task DisabledButProviderNotConfiguredShouldNotThrow()
        {
            CommandParserContext context = new(new TerminalRequest("id1", "prefix5_default"));
            CommandParser noProviderParser = new(routeParser, logger);
            await noProviderParser.ParseCommandAsync(context);
        }

        [Fact]
        public async Task CommandWithNoArgsShouldNotErrorAsync()
        {
            CommandParserContext context = new(new TerminalRequest("id1", "prefix4_noargs"));
            var result = await parser.ParseCommandAsync(context);
            result.ParsedCommand.Command.Options.Should().BeNull();
        }

        private readonly ICommandRequestParser routeParser = null!;
        private readonly CommandParser parser = null!;
        private readonly ILogger<CommandParser> logger = null!;
    }
}