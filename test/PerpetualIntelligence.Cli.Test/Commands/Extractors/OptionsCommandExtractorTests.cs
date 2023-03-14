/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Providers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Cli.Stores;
using PerpetualIntelligence.Cli.Stores.InMemory;

using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Extractors
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
            options.Extractor.DefaultArgumentValue = false;

            CommandExtractorContext context = new(new CommandString("pi     -key1_alias   value1  --key2-er  value2   --key6-a-s-xx-s --key9   25.36     -k12     "));
            var result = await extractor.ExtractAsync(context);

            AssertCommand(result.Command, "orgid", "pi", 5);

            Assert.IsNotNull(result.Command.Arguments);
            AssertArgument(result.Command.Arguments[0], "key1", DataType.Text, "Key1 value text", "value1", "key1_alias");
            AssertArgument(result.Command.Arguments[1], "key2-er", DataType.Text, "Key2 value text", "value2", null);
            AssertArgument(result.Command.Arguments[2], "key6-a-s-xx-s", nameof(Boolean), "Key6 no value", true.ToString(), null);
            AssertArgument(result.Command.Arguments[3], "key9", nameof(Double), "Key9 invalid default value", "25.36", null);
            AssertArgument(result.Command.Arguments[4], "key12", nameof(Boolean), "Key12 value default boolean", true.ToString(), "k12");
        }

        [TestMethod]
        public async Task AdditionalSeparatorsWithinValueShouldNotBeIgnored()
        {
            options.Extractor.DefaultArgumentValue = false;
            options.Extractor.ArgumentValueWithIn = "\"";

            CommandExtractorContext context = new(new CommandString("pi     -key1_alias \"  value1 \"  --key2-er  \"value2     \" --key6-a-s-xx-s --key9   25.36     -k12     "));
            var result = await extractor.ExtractAsync(context);

            AssertCommand(result.Command, "orgid", "pi", 5);

            Assert.IsNotNull(result.Command.Arguments);
            AssertArgument(result.Command.Arguments[0], "key1", DataType.Text, "Key1 value text", "  value1 ", "key1_alias");
            AssertArgument(result.Command.Arguments[1], "key2-er", DataType.Text, "Key2 value text", "value2     ", null);
            AssertArgument(result.Command.Arguments[2], "key6-a-s-xx-s", nameof(Boolean), "Key6 no value", true.ToString(), null);
            AssertArgument(result.Command.Arguments[3], "key9", nameof(Double), "Key9 invalid default value", "25.36", null);
            AssertArgument(result.Command.Arguments[4], "key12", nameof(Boolean), "Key12 value default boolean", true.ToString(), "k12");
        }

        [TestMethod]
        public async Task AliasIdButAliasNotConfiguredShouldError()
        {
            options.Extractor.ArgumentAlias = false;

            CommandExtractorContext context = new(new CommandString("pi -key1_alias value1"));

            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
               () => extractor.ExtractAsync(context),
               1,
               new[] { Errors.InvalidConfiguration },
               new[] { "The option extraction by alias prefix is not configured. argument_string=-key1_alias value1" }
           );
        }

        [TestMethod]
        public async Task AliasIdShouldExtractCorrectly()
        {
            CommandExtractorContext context = new(new CommandString("pi -key1_alias value1"));
            var result = await extractor.ExtractAsync(context);

            AssertCommand(result.Command, "orgid", "pi", 1);
            Assert.IsNotNull(result.Command.Arguments);
            AssertArgument(result.Command.Arguments[0], "key1", DataType.Text, "Key1 value text", "value1", "key1_alias");
            Assert.AreEqual("key1_alias", result.Command.Arguments[0].Alias);
        }

        [TestMethod]
        public async Task AliasIdWithArgumentPrefixShouldError()
        {
            CommandExtractorContext context = new(new CommandString("pi --key1_alias value1"));

            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
                () => extractor.ExtractAsync(context),
                1,
                new[] { Errors.UnsupportedArgument },
                new[] { "The option is not supported. option=key1_alias" }
            );
        }

        [TestMethod]
        public async Task ArgumentIdShouldExtractCorrectly()
        {
            CommandExtractorContext context = new(new CommandString("pi --key1 value1"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.Command.Arguments);
            Assert.AreEqual(1, result.Command.Arguments.Count);
            AssertArgument(result.Command.Arguments[0], "key1", DataType.Text, "Key1 value text", "value1", "key1_alias");
        }

        [TestMethod]
        public async Task ArgumentIdWithAliasPrefixAndAliasNotConfiguredMultipleSpacesShouldError()
        {
            options.Extractor.ArgumentAlias = false;

            CommandExtractorContext context = new(new CommandString("pi    -key1     value1 "));

            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
               () => extractor.ExtractAsync(context),
               1,
               new[] { Errors.InvalidConfiguration },
               new[] { "The option extraction by alias prefix is not configured. argument_string=-key1 value1" }
           );
        }

        [TestMethod]
        public async Task ArgumentIdWithAliasPrefixAndAliasNotConfiguredShouldError()
        {
            options.Extractor.ArgumentAlias = false;

            CommandExtractorContext context = new(new CommandString("pi -key1 value1"));

            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
               () => extractor.ExtractAsync(context),
               1,
               new[] { Errors.InvalidConfiguration },
               new[] { "The option extraction by alias prefix is not configured. argument_string=-key1 value1" }
           );
        }

        [TestMethod]
        public async Task ArgumentIdWithAliasPrefixShouldError()
        {
            CommandExtractorContext context = new(new CommandString("pi -key1 value1"));

            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
               () => extractor.ExtractAsync(context),
               1,
               new[] { Errors.UnsupportedArgument },
               new[] { "The option alias is not supported. option=key1" }
           );
        }

        [TestMethod]
        public async Task DefaultArgumentValueConfiguredShouldExtractCorrectly()
        {
            options.Extractor.DefaultArgumentValue = true;

            CommandExtractorContext context = new(new CommandString("pi --key1 value1 --key2-er value2 --key6-a-s-xx-s"));
            var result = await extractor.ExtractAsync(context);

            AssertCommand(result.Command, "orgid", "pi", 6);

            Assert.IsNotNull(result.Command.Arguments);
            AssertArgument(result.Command.Arguments[0], "key1", DataType.Text, "Key1 value text", "value1", "key1_alias");
            AssertArgument(result.Command.Arguments[1], "key2-er", DataType.Text, "Key2 value text", "value2", null);
            AssertArgument(result.Command.Arguments[2], "key6-a-s-xx-s", nameof(Boolean), "Key6 no value", true.ToString(), null);

            AssertArgument(result.Command.Arguments[3], "key7", DataType.Currency, "Key7 value currency", "INR", null);
            AssertArgument(result.Command.Arguments[4], "key9", nameof(Double), "Key9 invalid default value", 89568.36, null);
            AssertArgument(result.Command.Arguments[5], "key12", nameof(Boolean), "Key12 value default boolean", true, "k12");
        }

        [TestMethod]
        public async Task DefaultArgumentValueConfiguredShouldOverrideCorrectly()
        {
            options.Extractor.DefaultArgumentValue = true;

            CommandExtractorContext context = new(new CommandString("pi --key1 value1 --key2-er value2 --key6-a-s-xx-s --key9 25.36"));
            var result = await extractor.ExtractAsync(context);

            AssertCommand(result.Command, "orgid", "pi", 6);

            Assert.IsNotNull(result.Command.Arguments);
            AssertArgument(result.Command.Arguments[0], "key1", DataType.Text, "Key1 value text", "value1", "key1_alias");
            AssertArgument(result.Command.Arguments[1], "key2-er", DataType.Text, "Key2 value text", "value2", null);
            AssertArgument(result.Command.Arguments[2], "key6-a-s-xx-s", nameof(Boolean), "Key6 no value", true.ToString(), null);
            AssertArgument(result.Command.Arguments[3], "key9", nameof(Double), "Key9 invalid default value", "25.36", null); // Override default value

            AssertArgument(result.Command.Arguments[4], "key7", DataType.Currency, "Key7 value currency", "INR", null);
            AssertArgument(result.Command.Arguments[5], "key12", nameof(Boolean), "Key12 value default boolean", true, "k12");
        }

        [TestMethod]
        public async Task DefaultArgumentValueNotConfiguredShouldExtractCorrectly()
        {
            options.Extractor.DefaultArgumentValue = false;

            CommandExtractorContext context = new(new CommandString("pi --key1 value1 --key2-er value2 --key6-a-s-xx-s --key9 25.36"));
            var result = await extractor.ExtractAsync(context);

            AssertCommand(result.Command, "orgid", "pi", 4);

            Assert.IsNotNull(result.Command.Arguments);
            AssertArgument(result.Command.Arguments[0], "key1", DataType.Text, "Key1 value text", "value1", "key1_alias");
            AssertArgument(result.Command.Arguments[1], "key2-er", DataType.Text, "Key2 value text", "value2", null);
            AssertArgument(result.Command.Arguments[2], "key6-a-s-xx-s", nameof(Boolean), "Key6 no value", true.ToString(), null);
            AssertArgument(result.Command.Arguments[3], "key9", nameof(Double), "Key9 invalid default value", "25.36", null);
        }

        [TestMethod]
        public async Task MixedArgumentsAndAliasShouldExtractCorrectly()
        {
            options.Extractor.DefaultArgumentValue = false;

            CommandExtractorContext context = new(new CommandString("pi --key1 value1 --key2-er value2 -k3 (551) 208 9779 --key6-a-s-xx-s --key9 25.36 -k10 value10 -k11 --key12"));
            var result = await extractor.ExtractAsync(context);

            AssertCommand(result.Command, "orgid", "pi", 8);

            Assert.IsNotNull(result.Command.Arguments);
            AssertArgument(result.Command.Arguments[0], "key1", DataType.Text, "Key1 value text", "value1", "key1_alias");
            AssertArgument(result.Command.Arguments[1], "key2-er", DataType.Text, "Key2 value text", "value2", null);
            AssertArgument(result.Command.Arguments[2], "key3-a-z-d", DataType.PhoneNumber, "Key3 value phone", "(551) 208 9779", "k3");
            AssertArgument(result.Command.Arguments[3], "key6-a-s-xx-s", nameof(Boolean), "Key6 no value", true.ToString(), null);
            AssertArgument(result.Command.Arguments[4], "key9", nameof(Double), "Key9 invalid default value", "25.36", null);
            AssertArgument(result.Command.Arguments[5], "key10", nameof(String), "Key10 value custom string", "value10", "k10");
            AssertArgument(result.Command.Arguments[6], "key11", nameof(Boolean), "Key11 value boolean", true.ToString(), "k11");
            AssertArgument(result.Command.Arguments[7], "key12", nameof(Boolean), "Key12 value default boolean", true.ToString(), "k12");
        }

        [TestMethod]
        public async Task MixedArgumentsInRandomOrderAndAliasShouldExtractCorrectly()
        {
            options.Extractor.DefaultArgumentValue = false;

            CommandExtractorContext context = new(new CommandString("pi -k11 -k3 (551) 208 9779 --key9 25.36 --key2-er value2 --key12 --key6-a-s-xx-s --key1 value1 -k10 value10"));
            var result = await extractor.ExtractAsync(context);

            AssertCommand(result.Command, "orgid", "pi", 8);

            Assert.IsNotNull(result.Command.Arguments);
            AssertArgument(result.Command.Arguments[0], "key11", nameof(Boolean), "Key11 value boolean", true.ToString(), "k11");
            AssertArgument(result.Command.Arguments[1], "key3-a-z-d", DataType.PhoneNumber, "Key3 value phone", "(551) 208 9779", "k3");
            AssertArgument(result.Command.Arguments[2], "key9", nameof(Double), "Key9 invalid default value", "25.36", null);
            AssertArgument(result.Command.Arguments[3], "key2-er", DataType.Text, "Key2 value text", "value2", null);
            AssertArgument(result.Command.Arguments[4], "key12", nameof(Boolean), "Key12 value default boolean", true.ToString(), "k12");
            AssertArgument(result.Command.Arguments[5], "key6-a-s-xx-s", nameof(Boolean), "Key6 no value", true.ToString(), null);
            AssertArgument(result.Command.Arguments[6], "key1", DataType.Text, "Key1 value text", "value1", "key1_alias");
            AssertArgument(result.Command.Arguments[7], "key10", nameof(String), "Key10 value custom string", "value10", "k10");
        }

        [TestMethod]
        public async Task MixedArgumentsInReverseAndAliasShouldExtractCorrectly()
        {
            options.Extractor.DefaultArgumentValue = false;

            CommandExtractorContext context = new(new CommandString("pi --key12 -k11 -k10 value10 --key9 25.36 --key6-a-s-xx-s -k3 (551) 208 9779 --key2-er value2 --key1 value1"));
            var result = await extractor.ExtractAsync(context);

            AssertCommand(result.Command, "orgid", "pi", 8);

            Assert.IsNotNull(result.Command.Arguments);
            AssertArgument(result.Command.Arguments[7], "key1", DataType.Text, "Key1 value text", "value1", "key1_alias");
            AssertArgument(result.Command.Arguments[6], "key2-er", DataType.Text, "Key2 value text", "value2", null);
            AssertArgument(result.Command.Arguments[5], "key3-a-z-d", DataType.PhoneNumber, "Key3 value phone", "(551) 208 9779", "k3");
            AssertArgument(result.Command.Arguments[4], "key6-a-s-xx-s", nameof(Boolean), "Key6 no value", true.ToString(), null);
            AssertArgument(result.Command.Arguments[3], "key9", nameof(Double), "Key9 invalid default value", "25.36", null);
            AssertArgument(result.Command.Arguments[2], "key10", nameof(String), "Key10 value custom string", "value10", "k10");
            AssertArgument(result.Command.Arguments[1], "key11", nameof(Boolean), "Key11 value boolean", true.ToString(), "k11");
            AssertArgument(result.Command.Arguments[0], "key12", nameof(Boolean), "Key12 value default boolean", true.ToString(), "k12");
        }

        [TestMethod]
        public async Task NoArgumentsShouldExtractCorrectly()
        {
            options.Extractor.ArgumentAlias = false;

            CommandExtractorContext context = new(new CommandString("pi"));
            var result = await extractor.ExtractAsync(context);

            AssertCommand(result.Command, "orgid", "pi", null);
        }

        protected override void OnTestInitialize()
        {
            options = MockCliOptions.NewOptions();
            textHandler = new UnicodeTextHandler();
            argExtractor = new OptionExtractor(textHandler, options, TestLogger.Create<OptionExtractor>());
            commands = new InMemoryCommandStore(textHandler, MockCommands.GroupedOptionsCommands, options, TestLogger.Create<InMemoryCommandStore>());
            argExtractor = new OptionExtractor(textHandler, options, TestLogger.Create<OptionExtractor>());
            defaultArgValueProvider = new DefaultArgumentValueProvider(textHandler);
            defaultArgProvider = new DefaultArgumentProvider(options, TestLogger.Create<DefaultArgumentProvider>());
            extractor = new CommandExtractor(commands, argExtractor, textHandler, options, TestLogger.Create<CommandExtractor>(), defaultArgProvider, defaultArgValueProvider);
        }

        private void AssertArgument(Option arg, string name, string customDataType, string description, object value, string? alias)
        {
            Assert.AreEqual(arg.Id, name);
            Assert.AreEqual(arg.DataType, DataType.Custom);
            Assert.AreEqual(arg.CustomDataType, customDataType);
            Assert.AreEqual(arg.Description, description);
            Assert.AreEqual(arg.Value, value);
            Assert.AreEqual(arg.Alias, alias);
        }

        private void AssertArgument(Option arg, string name, DataType dataType, string description, object value, string? alias)
        {
            Assert.AreEqual(arg.Id, name);
            Assert.AreEqual(arg.DataType, dataType);
            Assert.IsNull(arg.CustomDataType);
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
                Assert.IsNull(command.Arguments);
            }
            else
            {
                Assert.IsNotNull(command.Arguments);
                Assert.AreEqual(argCount, command.Arguments.Count);
            }
        }

        private OptionExtractor argExtractor = null!;
        private ICommandStoreHandler commands = null!;
        private IDefaultArgumentProvider defaultArgProvider = null!;
        private IDefaultArgumentValueProvider defaultArgValueProvider = null!;
        private CommandExtractor extractor = null!;
        private CliOptions options = null!;
        private ITextHandler textHandler = null!;
    }
}
