/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Terminal.Stores;
using PerpetualIntelligence.Terminal.Stores.InMemory;

using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    [TestClass]
    public class OptionsCommandExtractorTests : LoggerTests<OptionsCommandExtractorTests>
    {
        public OptionsCommandExtractorTests()
        {
        }

        [TestMethod]
        public async Task AdditionalSeparatorsShouldBeIgnored()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "pi     -key1_alias   value1  --key2-er  value2   --key6-a-s-xx-s --key9   25.36     -k12     "));
            var result = await extractor.ExtractAsync(context);

            AssertCommand(result.ParsedCommand.Command, "orgid", "pi", 5);

            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            AssertOption(result.ParsedCommand.Command.Options[0], "key1", nameof(String), "Key1 value text", "value1", "key1_alias");
            AssertOption(result.ParsedCommand.Command.Options[1], "key2-er", nameof(String), "Key2 value text", "value2", null);
            AssertOption(result.ParsedCommand.Command.Options[2], "key6-a-s-xx-s", nameof(Boolean), "Key6 no value", true.ToString(), null);
            AssertOption(result.ParsedCommand.Command.Options[3], "key9", nameof(Double), "Key9 invalid default value", "25.36", null);
            AssertOption(result.ParsedCommand.Command.Options[4], "key12", nameof(Boolean), "Key12 value default boolean", true.ToString(), "k12");
        }

        [TestMethod]
        public async Task AdditionalSeparatorsWithinValueShouldNotBeIgnored()
        {
            options.Extractor.ValueDelimiter = "\"";

            CommandExtractorContext context = new(new CommandRoute("id1", "pi     -key1_alias \"  value1 \"  --key2-er  \"value2     \" --key6-a-s-xx-s --key9   25.36     -k12     "));
            var result = await extractor.ExtractAsync(context);

            AssertCommand(result.ParsedCommand.Command, "orgid", "pi", 5);

            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            AssertOption(result.ParsedCommand.Command.Options[0], "key1", nameof(String), "Key1 value text", "  value1 ", "key1_alias");
            AssertOption(result.ParsedCommand.Command.Options[1], "key2-er", nameof(String), "Key2 value text", "value2     ", null);
            AssertOption(result.ParsedCommand.Command.Options[2], "key6-a-s-xx-s", nameof(Boolean), "Key6 no value", true.ToString(), null);
            AssertOption(result.ParsedCommand.Command.Options[3], "key9", nameof(Double), "Key9 invalid default value", "25.36", null);
            AssertOption(result.ParsedCommand.Command.Options[4], "key12", nameof(Boolean), "Key12 value default boolean", true.ToString(), "k12");
        }

        [TestMethod]
        public async Task AliasIdButAliasNotConfiguredShouldError()
        {
            options.Extractor.OptionAlias = false;

            CommandExtractorContext context = new(new CommandRoute("id1", "pi -key1_alias value1"));

            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
               () => extractor.ExtractAsync(context),
               1,
               new[] { TerminalErrors.InvalidConfiguration },
               new[] { "The option extraction by alias prefix is not configured. option_string=-key1_alias value1" }
           );
        }

        [TestMethod]
        public async Task AliasIdShouldExtractCorrectly()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "pi -key1_alias value1"));
            var result = await extractor.ExtractAsync(context);

            AssertCommand(result.ParsedCommand.Command, "orgid", "pi", 1);
            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            AssertOption(result.ParsedCommand.Command.Options[0], "key1", nameof(String), "Key1 value text", "value1", "key1_alias");
            Assert.AreEqual("key1_alias", result.ParsedCommand.Command.Options[0].Alias);
        }

        [TestMethod]
        public async Task AliasIdWithOptionPrefixShouldError()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "pi --key1_alias value1"));

            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
                () => extractor.ExtractAsync(context),
                1,
                new[] { TerminalErrors.UnsupportedOption },
                new[] { "The option is not supported. option=key1_alias" }
            );
        }

        [TestMethod]
        public async Task ArgumentIdShouldExtractCorrectly()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "pi --key1 value1"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            Assert.AreEqual(1, result.ParsedCommand.Command.Options.Count);
            AssertOption(result.ParsedCommand.Command.Options[0], "key1", nameof(String), "Key1 value text", "value1", "key1_alias");
        }

        [TestMethod]
        public async Task ArgumentIdWithAliasPrefixAndAliasNotConfiguredMultipleSpacesShouldError()
        {
            options.Extractor.OptionAlias = false;

            CommandExtractorContext context = new(new CommandRoute("id1", "pi    -key1     value1 "));

            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
               () => extractor.ExtractAsync(context),
               1,
               new[] { TerminalErrors.InvalidConfiguration },
               new[] { "The option extraction by alias prefix is not configured. option_string=-key1 value1" }
           );
        }

        [TestMethod]
        public async Task ArgumentIdWithAliasPrefixAndAliasNotConfiguredShouldError()
        {
            options.Extractor.OptionAlias = false;

            CommandExtractorContext context = new(new CommandRoute("id1", "pi -key1 value1"));

            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
               () => extractor.ExtractAsync(context),
               1,
               new[] { TerminalErrors.InvalidConfiguration },
               new[] { "The option extraction by alias prefix is not configured. option_string=-key1 value1" }
           );
        }

        [TestMethod]
        public async Task ArgumentIdWithAliasPrefixShouldError()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "pi -key1 value1"));

            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
               () => extractor.ExtractAsync(context),
               1,
               new[] { TerminalErrors.UnsupportedOption },
               new[] { "The option alias is not supported. option=key1" }
           );
        }

        [TestMethod]
        public async Task MixedOptionsAndAliasShouldExtractCorrectly()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "pi --key1 value1 --key2-er value2 -k3 (551) 208 9779 --key6-a-s-xx-s --key9 25.36 -k10 value10 -k11 --key12"));
            var result = await extractor.ExtractAsync(context);

            AssertCommand(result.ParsedCommand.Command, "orgid", "pi", 8);

            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            AssertOption(result.ParsedCommand.Command.Options[0], "key1", nameof(String), "Key1 value text", "value1", "key1_alias");
            AssertOption(result.ParsedCommand.Command.Options[1], "key2-er", nameof(String), "Key2 value text", "value2", null);
            AssertOption(result.ParsedCommand.Command.Options[2], "key3-a-z-d", nameof(Int64), "Key3 value phone", "(551) 208 9779", "k3");
            AssertOption(result.ParsedCommand.Command.Options[3], "key6-a-s-xx-s", nameof(Boolean), "Key6 no value", true.ToString(), null);
            AssertOption(result.ParsedCommand.Command.Options[4], "key9", nameof(Double), "Key9 invalid default value", "25.36", null);
            AssertOption(result.ParsedCommand.Command.Options[5], "key10", nameof(String), "Key10 value custom string", "value10", "k10");
            AssertOption(result.ParsedCommand.Command.Options[6], "key11", nameof(Boolean), "Key11 value boolean", true.ToString(), "k11");
            AssertOption(result.ParsedCommand.Command.Options[7], "key12", nameof(Boolean), "Key12 value default boolean", true.ToString(), "k12");
        }

        [TestMethod]
        public async Task MixedOptionsInRandomOrderAndAliasShouldExtractCorrectly()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "pi -k11 -k3 (551) 208 9779 --key9 25.36 --key2-er value2 --key12 --key6-a-s-xx-s --key1 value1 -k10 value10"));
            var result = await extractor.ExtractAsync(context);

            AssertCommand(result.ParsedCommand.Command, "orgid", "pi", 8);

            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            AssertOption(result.ParsedCommand.Command.Options[0], "key11", nameof(Boolean), "Key11 value boolean", true.ToString(), "k11");
            AssertOption(result.ParsedCommand.Command.Options[1], "key3-a-z-d", nameof(Int64), "Key3 value phone", "(551) 208 9779", "k3");
            AssertOption(result.ParsedCommand.Command.Options[2], "key9", nameof(Double), "Key9 invalid default value", "25.36", null);
            AssertOption(result.ParsedCommand.Command.Options[3], "key2-er", nameof(String), "Key2 value text", "value2", null);
            AssertOption(result.ParsedCommand.Command.Options[4], "key12", nameof(Boolean), "Key12 value default boolean", true.ToString(), "k12");
            AssertOption(result.ParsedCommand.Command.Options[5], "key6-a-s-xx-s", nameof(Boolean), "Key6 no value", true.ToString(), null);
            AssertOption(result.ParsedCommand.Command.Options[6], "key1", nameof(String), "Key1 value text", "value1", "key1_alias");
            AssertOption(result.ParsedCommand.Command.Options[7], "key10", nameof(String), "Key10 value custom string", "value10", "k10");
        }

        [TestMethod]
        public async Task MixedOptionsInReverseAndAliasShouldExtractCorrectly()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "pi --key12 -k11 -k10 value10 --key9 25.36 --key6-a-s-xx-s -k3 (551) 208 9779 --key2-er value2 --key1 value1"));
            var result = await extractor.ExtractAsync(context);

            AssertCommand(result.ParsedCommand.Command, "orgid", "pi", 8);

            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            AssertOption(result.ParsedCommand.Command.Options[7], "key1", nameof(String), "Key1 value text", "value1", "key1_alias");
            AssertOption(result.ParsedCommand.Command.Options[6], "key2-er", nameof(String), "Key2 value text", "value2", null);
            AssertOption(result.ParsedCommand.Command.Options[5], "key3-a-z-d", nameof(Int64), "Key3 value phone", "(551) 208 9779", "k3");
            AssertOption(result.ParsedCommand.Command.Options[4], "key6-a-s-xx-s", nameof(Boolean), "Key6 no value", true.ToString(), null);
            AssertOption(result.ParsedCommand.Command.Options[3], "key9", nameof(Double), "Key9 invalid default value", "25.36", null);
            AssertOption(result.ParsedCommand.Command.Options[2], "key10", nameof(String), "Key10 value custom string", "value10", "k10");
            AssertOption(result.ParsedCommand.Command.Options[1], "key11", nameof(Boolean), "Key11 value boolean", true.ToString(), "k11");
            AssertOption(result.ParsedCommand.Command.Options[0], "key12", nameof(Boolean), "Key12 value default boolean", true.ToString(), "k12");
        }

        [TestMethod]
        public async Task NoOptionsShouldExtractCorrectly()
        {
            options.Extractor.OptionAlias = false;

            CommandExtractorContext context = new(new CommandRoute("id1", "pi"));
            var result = await extractor.ExtractAsync(context);

            AssertCommand(result.ParsedCommand.Command, "orgid", "pi", null);
        }

        protected override void OnTestInitialize()
        {
            options = MockTerminalOptions.NewAliasOptions();
            textHandler = new UnicodeTextHandler();
            routeParser = new MockCommandRouteParser();
            commandStore = new InMemoryCommandStore(MockCommands.GroupedOptionsCommands);
            extractor = new CommandExtractor(routeParser);
        }

        private void AssertOption(Option arg, string name, string dataType, string description, object value, string? alias)
        {
            Assert.AreEqual(arg.Id, name);
            Assert.AreEqual(arg.DataType, dataType);
            Assert.AreEqual(arg.Description, description);
            Assert.AreEqual(arg.Value, value);
            Assert.AreEqual(arg.Alias, alias);
        }

        private void AssertCommand(Command command, string cmdId, string cmdName, int? argCount)
        {
            Assert.AreEqual(cmdId, command.Id);
            Assert.AreEqual(cmdName, command.Name);

            if (argCount == null)
            {
                Assert.IsNull(command.Options);
            }
            else
            {
                Assert.IsNotNull(command.Options);
                Assert.AreEqual(argCount, command.Options.Count);
            }
        }

        private ICommandRouteParser routeParser = null!;
        private ICommandStoreHandler commandStore = null!;
        private CommandExtractor extractor = null!;
        private TerminalOptions options = null!;
        private ITextHandler textHandler = null!;
    }
}