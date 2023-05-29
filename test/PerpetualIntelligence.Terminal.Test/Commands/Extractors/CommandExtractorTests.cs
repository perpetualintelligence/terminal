/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Terminal.Commands.Checkers;
using PerpetualIntelligence.Terminal.Commands.Extractors.Mocks;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Commands.Providers;
using PerpetualIntelligence.Terminal.Commands.Runners;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Terminal.Stores;
using PerpetualIntelligence.Terminal.Stores.InMemory;

using PerpetualIntelligence.Shared.Attributes;
using PerpetualIntelligence.Shared.Attributes.Validation;
using PerpetualIntelligence.Shared.Infrastructure;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    [TestClass]
    public class CommandExtractorTests : InitializerTests
    {
        public CommandExtractorTests() : base(TestLogger.Create<CommandExtractorTests>())
        {
        }

        [DataTestMethod]
        [DataRow("pi", "orgid", "pi")]
        [DataRow("pi auth", "orgid:authid", "auth")]
        [DataRow("pi auth login", "orgid:authid:loginid", "login")]
        public async Task AliasConfiguredDifferentPrefixWithInvalidAliasShouldError(string prefix, string cmdId, string cmdName)
        {
            // Enable alias
            cliOptions.Extractor.OptionAlias = true;
            cliOptions.Extractor.OptionPrefix = "--";
            cliOptions.Extractor.OptionAliasPrefix = "-";

            // Reset commands
            commands = new InMemoryCommandStore(textHandler, MockCommands.AliasCommands, cliOptions, TestLogger.Create<InMemoryCommandStore>());
            extractor = new CommandExtractor(commands, argExtractor, textHandler, cliOptions, TestLogger.Create<CommandExtractor>(), null, null);

            CommandExtractorContext context = new(new CommandRoute("id1", $"{prefix} -key1_invalid_alias=value1 --key2=value2 -key3_alias=value3 --key4=25.36"));
            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
               () => extractor.ExtractAsync(context),
               1,
               new[] {
                    Errors.UnsupportedOption
               },
               new[] {
                    $"The option alias is not supported. option=key1_invalid_alias",
               });
        }

        [DataTestMethod]
        [DataRow("pi", "orgid", "pi")]
        [DataRow("pi auth", "orgid:authid", "auth")]
        [DataRow("pi auth login", "orgid:authid:loginid", "login")]
        public async Task AliasConfiguredDifferentPrefixWithInvalidArgShouldError(string prefix, string cmdId, string cmdName)
        {
            // Enable alias
            cliOptions.Extractor.OptionAlias = true;
            cliOptions.Extractor.OptionPrefix = "--";
            cliOptions.Extractor.OptionAliasPrefix = "-";

            // Reset commands
            commands = new InMemoryCommandStore(textHandler, MockCommands.AliasCommands, cliOptions, TestLogger.Create<InMemoryCommandStore>());
            extractor = new CommandExtractor(commands, argExtractor, textHandler, cliOptions, TestLogger.Create<CommandExtractor>(), null, null);

            CommandExtractorContext context = new(new CommandRoute("id1", $"{prefix} -key1_alias=value1 --key2=value2 -key3_alias=value3 --key4_invalid=25.36"));
            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
               () => extractor.ExtractAsync(context),
               1,
               new[] {
                    Errors.UnsupportedOption
               },
               new[] {
                    $"The option is not supported. option=key4_invalid",
               });
        }

        [DataTestMethod]
        [DataRow("pi", "orgid", "pi")]
        [DataRow("pi auth", "orgid:authid", "auth")]
        [DataRow("pi auth login", "orgid:authid:loginid", "login")]
        public async Task AliasConfiguredSamePrefixWithInvalidAliasShouldError(string prefix, string cmdId, string cmdName)
        {
            // Enable alias
            cliOptions.Extractor.OptionAlias = true;

            // Reset commands
            commands = new InMemoryCommandStore(textHandler, MockCommands.AliasCommands, cliOptions, TestLogger.Create<InMemoryCommandStore>());
            extractor = new CommandExtractor(commands, argExtractor, textHandler, cliOptions, TestLogger.Create<CommandExtractor>(), null, null);

            CommandExtractorContext context = new(new CommandRoute("id1", $"{prefix} -key1_invalid_alias=value1 -key2=value2 -key3_alias=value3 -key4=25.36"));
            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
               () => extractor.ExtractAsync(context),
               1,
               new[] {
                    Errors.UnsupportedOption
               },
               new[] {
                    $"The option or its alias is not supported. option=key1_invalid_alias",
               });
        }

        [DataTestMethod]
        [DataRow("pi", "orgid", "pi")]
        [DataRow("pi auth", "orgid:authid", "auth")]
        [DataRow("pi auth login", "orgid:authid:loginid", "login")]
        public async Task AliasConfiguredSamePrefixWithInvalidArgShouldError(string prefix, string cmdId, string cmdName)
        {
            // Enable alias
            cliOptions.Extractor.OptionAlias = true;

            // Reset commands
            commands = new InMemoryCommandStore(textHandler, MockCommands.AliasCommands, cliOptions, TestLogger.Create<InMemoryCommandStore>());
            extractor = new CommandExtractor(commands, argExtractor, textHandler, cliOptions, TestLogger.Create<CommandExtractor>(), null, null);

            CommandExtractorContext context = new(new CommandRoute("id1", $"{prefix} -key1_alias=value1 -key2_invalid=value2 -key3_alias=value3 -key4=25.36"));
            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
               () => extractor.ExtractAsync(context),
               1,
               new[] {
                    Errors.UnsupportedOption
               },
               new[] {
                    $"The option or its alias is not supported. option=key2_invalid",
               });
        }

        [DataTestMethod]
        [DataRow("pi", "orgid")]
        [DataRow("pi auth", "orgid:authid")]
        [DataRow("pi auth login", "orgid:authid:loginid")]
        public async Task AliasConfiguredValidCommandStringWithAliasShouldNotError(string prefix, string cmdId)
        {
            // Enable alias
            cliOptions.Extractor.OptionAlias = true;

            // Reset commands
            commands = new InMemoryCommandStore(textHandler, MockCommands.AliasCommands, cliOptions, TestLogger.Create<InMemoryCommandStore>());
            extractor = new CommandExtractor(commands, argExtractor, textHandler, cliOptions, TestLogger.Create<CommandExtractor>(), null, null);

            CommandExtractorContext context = new(new CommandRoute("id1", $"{prefix} -key1_alias=value1 -key2=value2 -key3_alias=value3 -key4=25.36"));
            var result = await extractor.ExtractAsync(context);

            Assert.AreEqual(prefix, result.Command.Descriptor.Prefix);
            Assert.AreEqual(cmdId, result.Command.Id);

            Assert.IsNotNull(result.Command.Options);
            AssertOption(result.Command.Options[0], "key1", DataType.Text, "Key1 value text", "value1");
            AssertOption(result.Command.Options[1], "key2", DataType.Text, "Key2 value text", "value2");
            AssertOption(result.Command.Options[2], "key3", DataType.PhoneNumber, "Key3 value phone", "value3");
            AssertOption(result.Command.Options[3], "key4", nameof(Double), "Key4 value number", "25.36");
        }

        [DataTestMethod]
        [DataRow("pi", "orgid")]
        [DataRow("pi auth", "orgid:authid")]
        [DataRow("pi auth login", "orgid:authid:loginid")]
        public async Task AliasConfiguredWithDefaultArgAndDefaultValueShouldNotErrorAsync(string prefix, string cmdId)
        {
            // Enable alias
            cliOptions.Extractor.OptionAlias = true;
            cliOptions.Extractor.DefaultOption = true;
            cliOptions.Extractor.DefaultOptionValue = true;

            // Reset commands
            commands = new InMemoryCommandStore(textHandler, MockCommands.AliasCommands, cliOptions, TestLogger.Create<InMemoryCommandStore>());
            extractor = new CommandExtractor(commands, argExtractor, textHandler, cliOptions, TestLogger.Create<CommandExtractor>(), defaultArgProvider, defaultArgValueProvider);

            // key1 with alias key1_alias is default arg with default value key1 default value
            CommandExtractorContext context = new(new CommandRoute("id1", $"{prefix} -key2=value2 -key3_alias=value3 -key4=25.36"));
            var result = await extractor.ExtractAsync(context);

            Assert.AreEqual(prefix, result.Command.Descriptor.Prefix);
            Assert.AreEqual(cmdId, result.Command.Id);

            Assert.IsNotNull(result.Command.Options);
            AssertOption(result.Command.Options[0], "key2", DataType.Text, "Key2 value text", "value2");
            AssertOption(result.Command.Options[1], "key3", DataType.PhoneNumber, "Key3 value phone", "value3");
            AssertOption(result.Command.Options[2], "key4", nameof(Double), "Key4 value number", "25.36");
            AssertOption(result.Command.Options[3], "key1", DataType.Text, "Key1 value text", "key1 default value");
        }

        [DataTestMethod]
        [DataRow("pi", "orgid")]
        public async Task AliasConfiguredWithDefaultArgShouldNotErrorAsync(string prefix, string cmdId)
        {
            // Enable alias
            cliOptions.Extractor.OptionAlias = true;
            cliOptions.Extractor.DefaultOption = true;

            // Reset commands
            commands = new InMemoryCommandStore(textHandler, MockCommands.AliasCommands, cliOptions, TestLogger.Create<InMemoryCommandStore>());
            extractor = new CommandExtractor(commands, argExtractor, textHandler, cliOptions, TestLogger.Create<CommandExtractor>(), defaultArgProvider, null);

            // key1 with alias key1_alias is default arg for value1
            CommandExtractorContext context = new(new CommandRoute("id1", $"{prefix} value1 -key2=value2 -key3_alias=value3 -key4=25.36"));
            var result = await extractor.ExtractAsync(context);

            Assert.AreEqual(prefix, result.Command.Descriptor.Prefix);
            Assert.AreEqual(cmdId, result.Command.Id);

            Assert.IsNotNull(result.Command.Options);
            AssertOption(result.Command.Options[0], "key1", DataType.Text, "Key1 value text", "value1");
            AssertOption(result.Command.Options[1], "key2", DataType.Text, "Key2 value text", "value2");
            AssertOption(result.Command.Options[2], "key3", DataType.PhoneNumber, "Key3 value phone", "value3");
            AssertOption(result.Command.Options[3], "key4", nameof(Double), "Key4 value number", "25.36");
        }

        [DataTestMethod]
        [DataRow("pi", "orgid", "pi")]
        [DataRow("pi auth", "orgid:authid", "auth")]
        [DataRow("pi auth login", "orgid:authid:loginid", "login")]
        public async Task AliasNotConfiguredWithAliasShouldErrorAsync(string prefix, string cmdId, String cmdName)
        {
            // Disable alias
            cliOptions.Extractor.OptionAlias = false;

            // Reset commands
            commands = new InMemoryCommandStore(textHandler, MockCommands.AliasCommands, cliOptions, TestLogger.Create<InMemoryCommandStore>());
            extractor = new CommandExtractor(commands, argExtractor, textHandler, cliOptions, TestLogger.Create<CommandExtractor>(), null, null);

            CommandExtractorContext context = new(new CommandRoute("id1", $"{prefix} -key1_alias=value1 -key2=value2 -key3_alias=value3 -key4=25.36"));

            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
                () => extractor.ExtractAsync(context),
                2,
                new[] {
                    Errors.UnsupportedOption,
                    Errors.UnsupportedOption
                },
                new[] {
                    $"The option is not supported. option=key1_alias",
                    $"The option is not supported. option=key3_alias"
                });
        }

        [TestMethod]
        public async Task BadCustomArgExtractorShouldErrorAsync()
        {
            CommandExtractorContext context = new CommandExtractorContext(new CommandRoute("id1", "prefix1 -key1=value1 -key2=value2"));
            var badExtractor = new CommandExtractor(commands, new MockBadOptionExtractor(), textHandler, cliOptions, TestLogger.Create<CommandExtractor>(), null, null);

            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
                () => badExtractor.ExtractAsync(context),
                2,
                new[] {
                    Error.Unexpected,
                    Error.Unexpected
                },
                new[] {
                    "The request resulted in an unexpected error. additonal_info=Value cannot be null. (Parameter 'option')",
                    "The request resulted in an unexpected error. additonal_info=Value cannot be null. (Parameter 'option')"
                });
        }

        [TestMethod]
        public async Task CommandDoesNotSupportArgsButUserPassedArgShouldErrorAsync()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", $"prefix4_noargs -key1=hello -key2=mello -key3 -key4=36.69"));

            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedOption, "The command does not support any options. command_name=name4 command_id=id4");
        }

        [DataTestMethod]
        [DataRow("pi auth", "orgid:authid", "auth")]
        [DataRow("pi auth login", "orgid:authid:loginid", "login")]
        public async Task CommandIdRegexMismatchShouldErrorAsync(string prefix, string cmdId, String cmdName)
        {
            // Disallow :
            cliOptions.Extractor.CommandIdRegex = "^[A-Za-z0-9_-]*$";

            // Reset commands
            commands = new InMemoryCommandStore(textHandler, MockCommands.AliasCommands, cliOptions, TestLogger.Create<InMemoryCommandStore>());
            extractor = new CommandExtractor(commands, argExtractor, textHandler, cliOptions, TestLogger.Create<CommandExtractor>(), null, null);

            CommandExtractorContext context = new(new CommandRoute("id1", $"{prefix} -key1=value1 -key2=value2"));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.InvalidCommand, $"The command identifier is not valid. command_id={cmdId} regex={cliOptions.Extractor.CommandIdRegex}");
        }

        [DataTestMethod]
        [DataRow(" ")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task CommandSeparatorAndOptionSeparatorCanBeSameAsync(string separator)
        {
            cliOptions.Extractor.Separator = separator;
            cliOptions.Extractor.OptionValueSeparator = separator;

            CommandExtractorContext context = new CommandExtractorContext(new CommandRoute("id1", $"prefix1{separator}-key1{separator}value1{separator}-key2{separator}value2{separator}-key6{separator}-key9{separator}26.36"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.Command.Options);
            Assert.AreEqual(4, result.Command.Options.Count);
            AssertOption(result.Command.Options[0], "key1", DataType.Text, "Key1 value text", "value1");
            AssertOption(result.Command.Options[1], "key2", DataType.Text, "Key2 value text", "value2");
            AssertOption(result.Command.Options[2], "key6", nameof(Boolean), "Key6 no value", "True");
            AssertOption(result.Command.Options[3], "key9", nameof(Double), "Key9 value custom double", "26.36");
        }

        [DataTestMethod]
        [DataRow("@")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task CommandStringShouldAllowOptionPrefixInOptionValueAsync(string prefix)
        {
            // E.g. if ' ' space is used as a command separator then the command string should allow spaces in the
            // option values
            // -> prefix1 -key=Test-prefix-message -key2=nosprefixmessage -key3=again-with-prefix
            cliOptions.Extractor.OptionPrefix = prefix;

            CommandExtractorContext context = new(new CommandRoute("id1", $"prefix1 {prefix}key1=Test{prefix}prefix{prefix}message {prefix}key2=nospacemessage {prefix}key6 {prefix}key10=Again{prefix}with{prefix}prefix"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command);
            Assert.IsNotNull(result.Command.Options);
            Assert.AreEqual(4, result.Command.Options.Count);
            AssertOption(result.Command.Options[0], "key1", DataType.Text, "Key1 value text", $"Test{prefix}prefix{prefix}message");
            AssertOption(result.Command.Options[1], "key2", DataType.Text, "Key2 value text", "nospacemessage");
            AssertOption(result.Command.Options[2], "key6", nameof(Boolean), "Key6 no value", "True");
            AssertOption(result.Command.Options[3], "key10", nameof(String), "Key10 value custom string", $"Again{prefix}with{prefix}prefix");
        }

        [DataTestMethod]
        [DataRow("@")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task CommandStringShouldAllowOptionSeparatorInOptionValueAsync(string separator)
        {
            // E.g. if ' ' space is used as a command separator then the command string should allow spaces in the
            // option values
            // -> prefix1 -key=Test=separator=message -key2=nosseparatormessage -key3=again=with=separator
            cliOptions.Extractor.OptionValueSeparator = separator;

            CommandExtractorContext context = new CommandExtractorContext(new CommandRoute("id1", $"prefix1 -key1{separator}Test{separator}separator{separator}message -key2{separator}nosseparatormessage -key6 -key10{separator}Again{separator}with{separator}separator"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command);
            Assert.IsNotNull(result.Command.Options);
            Assert.AreEqual(4, result.Command.Options.Count);
            AssertOption(result.Command.Options[0], "key1", DataType.Text, "Key1 value text", $"Test{separator}separator{separator}message");
            AssertOption(result.Command.Options[1], "key2", DataType.Text, "Key2 value text", "nosseparatormessage");
            AssertOption(result.Command.Options[2], "key6", nameof(Boolean), "Key6 no value", "True");
            AssertOption(result.Command.Options[3], "key10", nameof(String), "Key10 value custom string", $"Again{separator}with{separator}separator");
        }

        [DataTestMethod]
        [DataRow(" ")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task CommandStringShouldAllowCommandSeparatorInOptionValueAsync(string seperator)
        {
            // E.g. if ' ' space is used as a command separator then the command string should allow spaces in the
            // option values
            // -> prefix1 -key=Test space message -key2=nospacemessage -key3=Again with space
            cliOptions.Extractor.Separator = seperator;

            CommandExtractorContext context = new(new CommandRoute("id1", $"prefix1{seperator}-key1=Test{seperator}space{seperator}message{seperator}-key2=nospacemessage{seperator}-key6{seperator}-key10=Again{seperator}with{seperator}space"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command);
            Assert.IsNotNull(result.Command.Options);
            Assert.AreEqual(4, result.Command.Options.Count);
            AssertOption(result.Command.Options[0], "key1", DataType.Text, "Key1 value text", $"Test{seperator}space{seperator}message");
            AssertOption(result.Command.Options[1], "key2", DataType.Text, "Key2 value text", "nospacemessage");
            AssertOption(result.Command.Options[2], "key6", nameof(Boolean), "Key6 no value", "True");
            AssertOption(result.Command.Options[3], "key10", nameof(String), "Key10 value custom string", $"Again{seperator}with{seperator}space");
        }

        [DataTestMethod]
        [DataRow(" ")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task CommandStringShouldAllowMultipleSeparatorForArgumentAndOptionValueAsync(string seperator)
        {
            // E.g. if ' ' space is used as a command separator then the command string should allow multiple spaces in
            // the option and spaces in option values
            // -> "prefix1 -key1=Test space message -key2=nospacemessage -key6 -key10=Again with space"
            cliOptions.Extractor.Separator = seperator;

            CommandExtractorContext context = new(new CommandRoute("id1", $"prefix1{seperator}{seperator}{seperator}-key1=Test{seperator}{seperator}space{seperator}{seperator}{seperator}{seperator}message{seperator}{seperator}{seperator}-key2=nospacemessage{seperator}{seperator}-key6{seperator}{seperator}{seperator}{seperator}-key10=Again{seperator}with{seperator}{seperator}space"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command);
            Assert.IsNotNull(result.Command.Options);
            Assert.AreEqual(4, result.Command.Options.Count);
            AssertOption(result.Command.Options[0], "key1", DataType.Text, "Key1 value text", $"Test{seperator}{seperator}space{seperator}{seperator}{seperator}{seperator}message");
            AssertOption(result.Command.Options[1], "key2", DataType.Text, "Key2 value text", "nospacemessage");
            AssertOption(result.Command.Options[2], "key6", nameof(Boolean), "Key6 no value", "True");
            AssertOption(result.Command.Options[3], "key10", nameof(String), "Key10 value custom string", $"Again{seperator}with{seperator}{seperator}space");
        }

        [TestMethod]
        [DataRow(" ")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task CommandStringShouldStripSeparatorAtTheEndOfOptionValueAsync(string separator)
        {
            cliOptions.Extractor.Separator = separator;

            CommandExtractorContext context = new(new CommandRoute("id1", $"prefix1{separator}-key1=Test space message{separator}{separator}{separator}{separator}{separator}"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command);
            Assert.IsNotNull(result.Command.Options);
            Assert.AreEqual(1, result.Command.Options.Count);
            AssertOption(result.Command.Options[0], "key1", DataType.Text, "Key1 value text", $"Test space message");
        }

        [TestMethod]
        [WriteDocumentation]
        public async Task DefaultOptionShouldWorkCorrectlyWithItsValueAndWithInConfiguredAsync()
        {
            cliOptions.Extractor.DefaultOption = true;
            cliOptions.Extractor.OptionValueWithIn = "\"";

            // No arg id
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix7_defaultarg \"key1_value\""));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command.Options);
            AssertOption(result.Command.Options[0], "key1", DataType.Text, "Key1 value text", "key1_value");

            // with arg id
            context = new(new CommandRoute("id1", "prefix7_defaultarg -key1=\"key1_value\""));
            result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command.Options);
            AssertOption(result.Command.Options[0], "key1", DataType.Text, "Key1 value text", "key1_value");
        }

        [TestMethod]
        [WriteDocumentation]
        public async Task DefaultOptionShouldWorkCorrectlyWithItsValueAsync()
        {
            cliOptions.Extractor.DefaultOption = true;
            cliOptions.Extractor.OptionValueWithIn = null;

            // value without arg id
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix7_defaultarg key1_value"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command.Options);
            Assert.AreEqual(1, result.Command.Options.Count);
            AssertOption(result.Command.Options[0], "key1", DataType.Text, "Key1 value text", "key1_value");

            // value with arg id
            context = new(new CommandRoute("id1", "prefix7_defaultarg -key1=key1_value"));
            result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command.Options);
            Assert.AreEqual(1, result.Command.Options.Count);
            AssertOption(result.Command.Options[0], "key1", DataType.Text, "Key1 value text", "key1_value");
        }

        [TestMethod]
        [WriteDocumentation]
        public async Task DefaultOptionShouldWorkCorrectlyWithoutItsValueAsync()
        {
            cliOptions.Extractor.DefaultOption = true;

            // No default arg value
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix7_defaultarg"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNull(result.Command.Options);

            // No default arg value with other args
            context = new(new CommandRoute("id1", "prefix7_defaultarg -key2=hello -key6"));
            result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command.Options);
            Assert.AreEqual(2, result.Command.Options.Count);
            AssertOption(result.Command.Options[0], "key2", DataType.Text, "Key2 value text", "hello");
            AssertOption(result.Command.Options[1], "key6", nameof(Boolean), "Key6 no value", "True");
        }

        [TestMethod]
        [WriteDocumentation]
        public async Task DefaultOptionWithBothExplicitAndImplicitOptionValueShouldError()
        {
            cliOptions.Extractor.DefaultOption = true;
            cliOptions.Extractor.DefaultOptionValue = true;

            CommandExtractorContext context = new(new CommandRoute("id1", "prefix8_defaultarg_defaultvalue implitcit_default_value -key1=explicit_default_value"));
            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
                () => extractor.ExtractAsync(context),
                1,
                new[] { Errors.DuplicateOption },
                new[] { "The option is already added to the command. option=key1" }
                );
        }

        [TestMethod]
        [WriteDocumentation]
        public async Task DefaultOptionWithDefaultValueNotSpecifiedShouldWorkCorrectly()
        {
            cliOptions.Extractor.DefaultOption = true;
            cliOptions.Extractor.DefaultOptionValue = true;

            // No default arg, but it will be added because it has a default value
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix8_defaultarg_defaultvalue"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command.Options);
            AssertOption(result.Command.Options[0], "key1", DataType.Text, "Key1 value text", "key1 default value");

            // with arg id
            context = new(new CommandRoute("id1", "prefix8_defaultarg_defaultvalue -key2=key2_value -key3=3652669856"));
            result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command.Options);
            AssertOption(result.Command.Options[0], "key2", DataType.Text, "Key2 value text", "key2_value");
            AssertOption(result.Command.Options[1], "key3", DataType.PhoneNumber, "Key3 value phone", "3652669856");
            AssertOption(result.Command.Options[2], "key1", DataType.Text, "Key1 value text", "key1 default value");
        }

        [TestMethod]
        public async Task DefaultValueConfiguredButProviderNotConfiguredShouldThrow()
        {
            cliOptions.Extractor.DefaultOptionValue = true;

            CommandExtractorContext context = new(new CommandRoute("id1", "prefix5_default"));
            CommandExtractor noProviderExtrator = new(commands, argExtractor, textHandler, cliOptions, TestLogger.Create<CommandExtractor>(), null, null);

            await TestHelper.AssertThrowsErrorExceptionAsync(() => noProviderExtrator.ExtractAsync(context), Errors.InvalidConfiguration, "The option default value provider is missing in the service collection. provider_type=IDefaultOptionValueProvider");
        }

        [TestMethod]
        public async Task DefaultValueConfiguredButUnspecifiedRequiredValuesShouldNotError()
        {
            cliOptions.Extractor.DefaultOptionValue = true;

            // This is just extracting no checking
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix5_default"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.Command.Options);
            Assert.AreEqual(4, result.Command.Options.Count);
            Assert.AreEqual("44444444444", result.Command.Options[0].Value);
            Assert.AreEqual(false, result.Command.Options[1].Value);
            Assert.AreEqual(25.36, result.Command.Options[2].Value);
            Assert.AreEqual("mello default", result.Command.Options[3].Value);
        }

        [TestMethod]
        public async Task DefaultValueConfiguredButUnspecifiedRequiredValuesShouldNotOverrideUserValues()
        {
            cliOptions.Extractor.DefaultOptionValue = true;

            // This is just extracting no checking
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix5_default -key6 -key10=user value"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.Command.Options);
            Assert.AreEqual(4, result.Command.Options.Count);

            // Option values are processed sequentially and default values are added at the end User values
            Assert.AreEqual("key6", result.Command.Options[0].Id);
            Assert.AreEqual("True", result.Command.Options[0].Value);
            Assert.AreEqual("key10", result.Command.Options[1].Id);
            Assert.AreEqual("user value", result.Command.Options[1].Value);

            // Default value are added in the end
            Assert.AreEqual("key3", result.Command.Options[2].Id);
            Assert.AreEqual("44444444444", result.Command.Options[2].Value);
            Assert.AreEqual("key9", result.Command.Options[3].Id);
            Assert.AreEqual(25.36, result.Command.Options[3].Value);
        }

        [TestMethod]
        public async Task DefaultValueConfiguredButUnspecifiedRequiredValuesShouldNotPopulateIfDisabled()
        {
            cliOptions.Extractor.DefaultOptionValue = false;

            // This is just extracting no checking
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix5_default"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNull(result.Command.Options);
        }

        [TestMethod]
        public async Task DefaultValueConfiguredCommandWithEmptyArgsShouldNotErrorAsync()
        {
            cliOptions.Extractor.DefaultOptionValue = true;

            CommandExtractorContext context = new(new CommandRoute("id1", "prefix6_empty_args"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNull(result.Command.Options);
        }

        [TestMethod]
        public async Task DefaultValueConfiguredCommandWithNoArgsShouldNotErrorAsync()
        {
            cliOptions.Extractor.DefaultOptionValue = true;

            CommandExtractorContext context = new(new CommandRoute("id1", "prefix4_noargs"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNull(result.Command.Options);
        }

        [TestMethod]
        public async Task DefaultValueDisabledButProviderNotConfiguredShouldNotThrow()
        {
            cliOptions.Extractor.DefaultOptionValue = false;

            CommandExtractorContext context = new(new CommandRoute("id1", "prefix5_default"));
            CommandExtractor noProviderExtrator = new(commands, argExtractor, textHandler, cliOptions, TestLogger.Create<CommandExtractor>(), null, null);
            await noProviderExtrator.ExtractAsync(context);
        }

        [TestMethod]
        public async Task DefaultValueNotConfiguredCommandWithNoArgsShouldNotErrorAsync()
        {
            cliOptions.Extractor.DefaultOptionValue = false;

            CommandExtractorContext context = new(new CommandRoute("id1", "prefix4_noargs"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNull(result.Command.Options);
        }

        [TestMethod]
        public async Task InvalidCommandStringWithinCommandGroupShouldErrorAsync()
        {
            // Reset commands
            commands = new InMemoryCommandStore(textHandler, MockCommands.GroupedCommands, cliOptions, TestLogger.Create<InMemoryCommandStore>());
            extractor = new CommandExtractor(commands, argExtractor, textHandler, cliOptions, TestLogger.Create<CommandExtractor>(), null, null);

            CommandExtractorContext context = new(new CommandRoute("id1", "pi auth invalid"));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedCommand, "The command prefix is not valid. prefix=pi auth invalid");

            context = new CommandExtractorContext(new CommandRoute("id1", "pi auth invalid -key1=value1"));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedCommand, "The command prefix is not valid. prefix=pi auth invalid");
        }

        [TestMethod]
        public async Task MixedSupportedAndUnsupportedArgsShouldErrorAsync()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", $"prefix1 -key1=hello -keyunsupported1 -key2=mello -key6 -key4=36.69 -keyunsupported2=value"));

            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
               () => extractor.ExtractAsync(context),
               2,
               new[] {
                    Errors.UnsupportedOption,
                    Errors.UnsupportedOption
               },
               new[] {
                    "The option is not supported. option=keyunsupported1",
                    "The option is not supported. option=keyunsupported2"
               });
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

        [TestMethod]
        public async Task PrefixMatchButDifferentCommandNameShouldNotError()
        {
            CommandExtractorContext context = new CommandExtractorContext(new CommandRoute("id1", "prefix1 -key1=value1 -key2=value2"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command.Descriptor);
            Assert.AreEqual("id1", result.Command.Descriptor.Id);
            Assert.AreEqual("name1", result.Command.Descriptor.Name);
            Assert.AreEqual("prefix1", result.Command.Descriptor.Prefix);
        }

        [TestMethod]
        public async Task PrefixMatchSameCommandNameShouldNotError()
        {
            CommandExtractorContext context = new CommandExtractorContext(new CommandRoute("id1", "name2 -key1=value1 -key2=value2"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command.Descriptor);
            Assert.AreEqual("id2", result.Command.Descriptor.Id);
            Assert.AreEqual("name2", result.Command.Descriptor.Name);
            Assert.AreEqual("name2", result.Command.Descriptor.Prefix);
        }

        [TestMethod]
        public async Task PrefixMatchWithMultipleWordsShouldNotError()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix3 sub3 name3 -key1=value1 -key2=value2"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command.Descriptor);
            Assert.AreEqual("id3", result.Command.Descriptor.Id);
            Assert.AreEqual("name3", result.Command.Descriptor.Name);
            Assert.AreEqual("prefix3 sub3 name3", result.Command.Descriptor.Prefix);
        }

        [TestMethod]
        public async Task PrefixMismatchMultipleWordsShouldError()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "invalid_prefix3 sub3 name3 -key1=value1 -key2=value2"));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedCommand, "The command prefix is not valid. prefix=invalid_prefix3 sub3 name3");
        }

        [TestMethod]
        public async Task PrefixMismatchShouldError()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "invalidprefix -key1=value1 -key2=value2"));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedCommand, "The command prefix is not valid. prefix=invalidprefix");
        }

        [TestMethod]
        public async Task TryMatchByPrefixShouldErrorIfNotFoundAsync()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "invalid prefix1"));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedCommand, "The command prefix is not valid. prefix=invalid prefix1");
        }

        [TestMethod]
        public async Task TryMatchByPrefixWithInsufficientPharasesShouldErrorAsync()
        {
            // Missing name3
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix3 sub3"));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedCommand, "The command prefix is not valid. prefix=prefix3 sub3");
        }

        [TestMethod]
        public async Task TryMatchByPrefixWithMultiplePharasesShouldNotErrorIfMatchedAsync()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix3 sub3 name3"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command.Descriptor);
            Assert.AreEqual("id3", result.Command.Descriptor.Id);
            Assert.AreEqual("name3", result.Command.Descriptor.Name);
            Assert.AreEqual("prefix3 sub3 name3", result.Command.Descriptor.Prefix);
        }

        [TestMethod]
        public async Task UnsupportedOptionsShouldErrorMultipleAsync()
        {
            CommandExtractorContext context = new CommandExtractorContext(new CommandRoute("id1", "prefix1 -invalid_key1=value1 -invalid_key2=value2 -invalid_key3=value3"));
            await TestHelper.AssertThrowsMultiErrorExceptionAsync
            (
                () => extractor.ExtractAsync(context),
                3,
                new[] {
                    Errors.UnsupportedOption,
                    Errors.UnsupportedOption,
                    Errors.UnsupportedOption
                },
                new[] {
                    "The option is not supported. option=invalid_key1",
                    "The option is not supported. option=invalid_key2",
                    "The option is not supported. option=invalid_key3"
                }
            );
        }

        [TestMethod]
        public async Task UnsupportedCommandShouldErrorAsync()
        {
            CommandExtractorContext context = new CommandExtractorContext(new CommandRoute("id1", "invalid_cmd"));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedCommand, "The command prefix is not valid. prefix=invalid_cmd");
        }

        [DataTestMethod]
        [DataRow(" ")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task ValidCommandStringShouldNotErrorAndPopulateCorrectly(string seperator)
        {
            cliOptions.Extractor.Separator = seperator;

            CommandExtractorContext context = new CommandExtractorContext(new CommandRoute("id1", $"name2{seperator}-key1=value1{seperator}-key2=value2{seperator}-key3=+1-2365985632{seperator}-key4=testmail@gmail.com{seperator}-key5=C:\\apps\\devop_tools\\bin\\wntx64\\i18nnotes.txt{seperator}-key6{seperator}-key9=33.368"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.Command.Descriptor);
            Assert.AreEqual("id2", result.Command.Descriptor.Id);
            Assert.AreEqual("name2", result.Command.Descriptor.Name);
            Assert.AreEqual("name2", result.Command.Descriptor.Prefix);
            Assert.AreEqual("desc2", result.Command.Descriptor.Description);
            Assert.AreEqual(typeof(CommandChecker), result.Command.Descriptor.Checker);
            Assert.AreEqual(typeof(CommandRunner<CommandRunnerResult>), result.Command.Descriptor.Runner);
            Assert.IsNotNull(result.Command.Descriptor.OptionDescriptors);
            Assert.AreEqual(10, result.Command.Descriptor.OptionDescriptors.Count);
            AssertOptionDescriptor(result.Command.Descriptor.OptionDescriptors[0], "key1", DataType.Text, "Key1 value text");
            AssertOptionDescriptor(result.Command.Descriptor.OptionDescriptors[1], "key2", DataType.Text, "Key2 value text");
            AssertOptionDescriptor(result.Command.Descriptor.OptionDescriptors[2], "key3", DataType.PhoneNumber, "Key3 value phone");
            AssertOptionDescriptor(result.Command.Descriptor.OptionDescriptors[3], "key4", DataType.EmailAddress, "Key4 value email");
            AssertOptionDescriptor(result.Command.Descriptor.OptionDescriptors[4], "key5", DataType.Url, "Key5 value url");
            AssertOptionIdentity(result.Command.Descriptor.OptionDescriptors[5], "key6", nameof(Boolean), "Key6 no value");
            AssertOptionDescriptor(result.Command.Descriptor.OptionDescriptors[6], "key7", DataType.Currency, "Key7 value currency", new DataValidationOptionValueChecker[] { new(new OneOfAttribute("INR", "USD", "EUR")) });
            AssertOptionIdentity(result.Command.Descriptor.OptionDescriptors[7], "key8", nameof(Int32), "Key8 value custom int");
            AssertOptionIdentity(result.Command.Descriptor.OptionDescriptors[8], "key9", nameof(Double), "Key9 value custom double", new DataValidationOptionValueChecker[] { new(new RequiredAttribute()), new(new OneOfAttribute(2.36, 25.36, 3669566.36, 26.36, -36985.25, 0, -5)) });
            AssertOptionIdentity(result.Command.Descriptor.OptionDescriptors[9], "key10", nameof(String), "Key10 value custom string");

            // Supported options 10, user only passed 7
            Assert.IsNotNull(result.Command);
            Assert.AreEqual("id2", result.Command.Id);
            Assert.AreEqual("name2", result.Command.Name);
            Assert.AreEqual("desc2", result.Command.Description);
            Assert.IsNotNull(result.Command.Options);
            Assert.AreEqual(7, result.Command.Options.Count);
            AssertOption(result.Command.Options[0], "key1", DataType.Text, "Key1 value text", "value1");
            AssertOption(result.Command.Options[1], "key2", DataType.Text, "Key2 value text", "value2");
            AssertOption(result.Command.Options[2], "key3", DataType.PhoneNumber, "Key3 value phone", "+1-2365985632");
            AssertOption(result.Command.Options[3], "key4", DataType.EmailAddress, "Key4 value email", "testmail@gmail.com");
            AssertOption(result.Command.Options[4], "key5", DataType.Url, "Key5 value url", "C:\\apps\\devop_tools\\bin\\wntx64\\i18nnotes.txt");
            AssertOption(result.Command.Options[5], "key6", "Boolean", "Key6 no value", "True");
            AssertOption(result.Command.Options[6], "key9", "Double", "Key9 value custom double", "33.368");
        }

        [TestMethod]
        public async Task ValidCommandStringWithOptionsButNoOptionsPassedShouldNotError()
        {
            CommandExtractorContext context = new CommandExtractorContext(new CommandRoute("id1", "prefix1"));
            var result = await extractor.ExtractAsync(context);

            // Args Supported
            Assert.IsNotNull(result.Command.Descriptor);
            Assert.IsNotNull(result.Command.Descriptor.OptionDescriptors);
            Assert.AreEqual(10, result.Command.Descriptor.OptionDescriptors.Count);

            // No Args Passed
            Assert.IsNotNull(result.Command);
            Assert.IsNull(result.Command.Options);
        }

        [TestMethod]
        public async Task ValidCommandStringWithinCommandGroupShouldNotErrorAsync()
        {
            // Reset commands
            commands = new InMemoryCommandStore(textHandler, MockCommands.GroupedCommands, cliOptions, TestLogger.Create<InMemoryCommandStore>());
            extractor = new CommandExtractor(commands, argExtractor, textHandler, cliOptions, TestLogger.Create<CommandExtractor>(), null, null);

            CommandExtractorContext context = new CommandExtractorContext(new CommandRoute("id1", "pi"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command.Descriptor);
            Assert.AreEqual("orgid", result.Command.Descriptor.Id);
            Assert.AreEqual("pi", result.Command.Descriptor.Name);

            context = new CommandExtractorContext(new CommandRoute("id1", "pi auth"));
            result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command.Descriptor);
            Assert.AreEqual("orgid:authid", result.Command.Descriptor.Id);
            Assert.AreEqual("auth", result.Command.Descriptor.Name);

            context = new CommandExtractorContext(new CommandRoute("id1", "pi auth login"));
            result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command.Descriptor);
            Assert.AreEqual("orgid:authid:loginid", result.Command.Descriptor.Id);
            Assert.AreEqual("login", result.Command.Descriptor.Name);

            context = new CommandExtractorContext(new CommandRoute("id1", "pi auth slogin"));
            result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command.Descriptor);
            Assert.AreEqual("orgid:authid:sloginid", result.Command.Descriptor.Id);
            Assert.AreEqual("slogin", result.Command.Descriptor.Name);

            context = new CommandExtractorContext(new CommandRoute("id1", "pi auth slogin oidc"));
            result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command.Descriptor);
            Assert.AreEqual("orgid:authid:sloginid:oidc", result.Command.Descriptor.Id);
            Assert.AreEqual("oidc", result.Command.Descriptor.Name);

            context = new CommandExtractorContext(new CommandRoute("id1", "pi auth slogin oauth"));
            result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command.Descriptor);
            Assert.AreEqual("orgid:authid:sloginid:oauth", result.Command.Descriptor.Id);
            Assert.AreEqual("oauth", result.Command.Descriptor.Name);
        }

        [TestMethod]
        public async Task ValidCommandStringWithNoOptionsShouldNotErrorAsync()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix4_noargs"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command);
            Assert.IsNull(result.Command.Options);
        }

        [TestMethod]
        public async Task ValidCommandStringWithNoSeparatorShouldErrorAsync()
        {
            CommandExtractorContext context = new CommandExtractorContext(new CommandRoute("id1", "prefix1-key1=value1-key2=value2"));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.InvalidCommand, "The command separator is missing. command_string=prefix1-key1=value1-key2=value2");
        }

        protected override void OnTestInitialize()
        {
            cliOptions = MockCliOptions.New();
            textHandler = new UnicodeTextHandler();
            commands = new InMemoryCommandStore(textHandler, MockCommands.Commands, cliOptions, TestLogger.Create<InMemoryCommandStore>());
            argExtractor = new OptionExtractor(textHandler, cliOptions, TestLogger.Create<OptionExtractor>());
            defaultArgValueProvider = new DefaultOptionValueProvider(textHandler);
            defaultArgProvider = new DefaultOptionProvider(cliOptions, TestLogger.Create<DefaultOptionProvider>());
            extractor = new CommandExtractor(commands, argExtractor, textHandler, cliOptions, TestLogger.Create<CommandExtractor>(), defaultArgProvider, defaultArgValueProvider);
        }

        private void AssertOption(Option arg, string name, string customDataType, string description, object value)
        {
            Assert.AreEqual(arg.Id, name);
            Assert.AreEqual(arg.DataType, DataType.Custom);
            Assert.AreEqual(arg.CustomDataType, customDataType);
            Assert.AreEqual(arg.Description, description);
            Assert.AreEqual(arg.Value, value);
        }

        private void AssertOption(Option arg, string name, DataType dataType, string description, object value)
        {
            Assert.AreEqual(arg.Id, name);
            Assert.AreEqual(arg.DataType, dataType);
            Assert.IsNull(arg.CustomDataType);
            Assert.AreEqual(arg.Description, description);
            Assert.AreEqual(arg.Value, value);
        }

        private void AssertOptionDescriptor(OptionDescriptor arg, string name, DataType dataType, string? description = null, DataValidationOptionValueChecker[]? supportedValues = null)
        {
            Assert.AreEqual(arg.Id, name);
            Assert.AreEqual(arg.DataType, dataType);
            Assert.IsNull(arg.CustomDataType);
            Assert.AreEqual(arg.Description, description);

            DataValidationOptionValueChecker[]? expectedCheckers = arg.ValueCheckers?.Cast<DataValidationOptionValueChecker>().ToArray();
            CollectionAssert.AreEquivalent(expectedCheckers, supportedValues);
        }

        private void AssertOptionIdentity(OptionDescriptor arg, string name, string customDataType, string? description = null, DataValidationOptionValueChecker[]? supportedValues = null)
        {
            Assert.AreEqual(arg.Id, name);
            Assert.AreEqual(arg.DataType, DataType.Custom);
            Assert.AreEqual(arg.CustomDataType, customDataType);
            Assert.AreEqual(arg.Description, description);
            CollectionAssert.AreEquivalent(arg.ValueCheckers?.Cast<DataValidationOptionValueChecker>().ToArray(), supportedValues);
        }

        private OptionExtractor argExtractor = null!;
        private ICommandStoreHandler commands = null!;
        private IDefaultOptionProvider defaultArgProvider = null!;
        private IDefaultOptionValueProvider defaultArgValueProvider = null!;
        private CommandExtractor extractor = null!;
        private CliOptions cliOptions = null!;
        private ITextHandler textHandler = null!;
    }
}