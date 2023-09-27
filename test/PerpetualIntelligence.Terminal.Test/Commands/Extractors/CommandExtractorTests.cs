﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Terminal.Stores;
using PerpetualIntelligence.Terminal.Stores.InMemory;
using PerpetualIntelligence.Test.Services;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    [TestClass]
    public class CommandExtractorTests
    {
        public CommandExtractorTests()
        {
            terminalOptions = MockTerminalOptions.NewLegacyOptions();
            textHandler = new UnicodeTextHandler();
            routeParser = new MockCommandRouteParser();
            commandStore = new InMemoryCommandStore(textHandler, MockCommands.Commands.Values);
            extractor = new CommandExtractor(routeParser);
        }

        [TestMethod]
        public async Task Calls_Route_ParserAsync()
        {
            MockCommandRouteParser routeParser = new();
            CommandExtractor extractor = new(routeParser);

            CommandExtractorContext context = new(new CommandRoute("id1", "id1 test raw string"));
            await extractor.ExtractAsync(context);

            routeParser.Called.Should().BeTrue();
            routeParser.PassedCommandRoute.Command.Raw.Should().Be("id1 test raw string");
        }

        [TestMethod]
        public async Task UnspecifiedRequiredValuesShouldNotPopulateIfDisabled()
        {
            // This is just extracting no checking
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix5_default"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNull(result.ParsedCommand.Command.Options);
        }

        [TestMethod]
        public async Task ConfiguredCommandWithNoArgsShouldNotErrorAsync()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix4_noargs"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNull(result.ParsedCommand.Command.Options);
        }

        [TestMethod]
        public async Task DisabledButProviderNotConfiguredShouldNotThrow()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix5_default"));
            CommandExtractor noProviderExtractor = new(routeParser);
            await noProviderExtractor.ExtractAsync(context);
        }

        [TestMethod]
        public async Task CommandWithNoArgsShouldNotErrorAsync()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix4_noargs"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNull(result.ParsedCommand.Command.Options);
        }

        [TestMethod]
        public void NullOrEmptyCommandStringShouldThrow()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CA1806 // Do not ignore method results
            TestHelper.AssertThrowsWithMessage<ArgumentException>(() => new CommandString(null), "'raw' cannot be null or whitespace. (Parameter 'raw')");
            TestHelper.AssertThrowsWithMessage<ArgumentException>(() => new CommandString("   "), "'raw' cannot be null or whitespace. (Parameter 'raw')");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CA1806 // Do not ignore method results
        }

        private ICommandStoreHandler commandStore = null!;
        private ICommandRouteParser routeParser = null!;
        private CommandExtractor extractor = null!;
        private TerminalOptions terminalOptions = null!;
        private ITextHandler textHandler = null!;
    }
}