/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Shared.Attributes;
using PerpetualIntelligence.Shared.Attributes.Validation;
using PerpetualIntelligence.Shared.Infrastructure;
using PerpetualIntelligence.Terminal.Commands.Checkers;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Commands.Runners;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Terminal.Stores;
using PerpetualIntelligence.Terminal.Stores.InMemory;
using PerpetualIntelligence.Test.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    [TestClass]
    public class CommandExtractorTests
    {
        public CommandExtractorTests()
        {
            terminalOptions = MockTerminalOptions.NewLegacyOptions();
            SetupBasedOnTerminalOptions(terminalOptions);
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

        [DataTestMethod]
        [DataRow("pi", "orgid", "pi")]
        [DataRow("pi auth", "orgid:authid", "auth")]
        [DataRow("pi auth login", "orgid:authid:loginid", "login")]
        public async Task AliasConfiguredDifferentPrefixValueDelimiterValidAliasShouldError(string prefix, string cmdId, string cmdName)
        {
            // Enable alias
            terminalOptions.Extractor.OptionAlias = true;
            terminalOptions.Extractor.OptionPrefix = "--";
            terminalOptions.Extractor.OptionAliasPrefix = "-";

            // Reset commands
            commandStore = new InMemoryCommandStore(MockCommands.AliasCommands);
            extractor = new CommandExtractor(routeParser);

            CommandExtractorContext context = new(new CommandRoute("id1", $"{prefix} -key1_invalid_alias=value1 --key2=value2 -key3_alias=value3 --key4=25.36"));
            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
               () => extractor.ExtractAsync(context),
               1,
               new[] {
                    TerminalErrors.UnsupportedOption
               },
               new[] {
                    $"The option alias is not supported. option=key1_invalid_alias",
               });
        }

        [DataTestMethod]
        [DataRow("pi", "orgid", "pi")]
        [DataRow("pi auth", "orgid:authid", "auth")]
        [DataRow("pi auth login", "orgid:authid:loginid", "login")]
        public async Task AliasConfiguredDifferentPrefixValueDelimiterValidArgShouldError(string prefix, string cmdId, string cmdName)
        {
            // Enable alias
            terminalOptions.Extractor.OptionAlias = true;
            terminalOptions.Extractor.OptionPrefix = "--";
            terminalOptions.Extractor.OptionAliasPrefix = "-";

            // Reset commands
            commandStore = new InMemoryCommandStore(MockCommands.AliasCommands);
            extractor = new CommandExtractor(routeParser);

            CommandExtractorContext context = new(new CommandRoute("id1", $"{prefix} -key1_alias=value1 --key2=value2 -key3_alias=value3 --key4_invalid=25.36"));
            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
               () => extractor.ExtractAsync(context),
               1,
               new[] {
                    TerminalErrors.UnsupportedOption
               },
               new[] {
                    $"The option is not supported. option=key4_invalid",
               });
        }

        [DataTestMethod]
        [DataRow("pi", "orgid", "pi")]
        [DataRow("pi auth", "orgid:authid", "auth")]
        [DataRow("pi auth login", "orgid:authid:loginid", "login")]
        public async Task AliasConfiguredSamePrefixValueDelimiterValidAliasShouldError(string prefix, string cmdId, string cmdName)
        {
            // Enable alias
            terminalOptions.Extractor.OptionAlias = true;

            // Reset commands
            commandStore = new InMemoryCommandStore(MockCommands.AliasCommands);
            extractor = new CommandExtractor(routeParser);

            CommandExtractorContext context = new(new CommandRoute("id1", $"{prefix} -key1_invalid_alias=value1 -key2=value2 -key3_alias=value3 -key4=25.36"));
            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
               () => extractor.ExtractAsync(context),
               1,
               new[] {
                    TerminalErrors.UnsupportedOption
               },
               new[] {
                    $"The option or its alias is not supported. option=key1_invalid_alias",
               });
        }

        [DataTestMethod]
        [DataRow("pi", "orgid", "pi")]
        [DataRow("pi auth", "orgid:authid", "auth")]
        [DataRow("pi auth login", "orgid:authid:loginid", "login")]
        public async Task AliasConfiguredSamePrefixValueDelimiterValidArgShouldError(string prefix, string cmdId, string cmdName)
        {
            // Enable alias
            terminalOptions.Extractor.OptionAlias = true;

            // Reset commands
            commandStore = new InMemoryCommandStore(MockCommands.AliasCommands);
            extractor = new CommandExtractor(routeParser);

            CommandExtractorContext context = new(new CommandRoute("id1", $"{prefix} -key1_alias=value1 -key2_invalid=value2 -key3_alias=value3 -key4=25.36"));
            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
               () => extractor.ExtractAsync(context),
               1,
               new[] {
                    TerminalErrors.UnsupportedOption
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
            terminalOptions.Extractor.OptionAlias = true;

            // Reset commands
            commandStore = new InMemoryCommandStore(MockCommands.AliasCommands);
            extractor = new CommandExtractor(routeParser);

            CommandExtractorContext context = new(new CommandRoute("id1", $"{prefix} -key1_alias=value1 -key2=value2 -key3_alias=value3 -key4=25.36"));
            var result = await extractor.ExtractAsync(context);

            Assert.AreEqual(cmdId, result.ParsedCommand.Command.Id);

            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            AssertOption(result.ParsedCommand.Command.Options["key1"], "key1", nameof(String), "Key1 value text", "value1");
            AssertOption(result.ParsedCommand.Command.Options["key2"], "key2", nameof(String), "Key2 value text", "value2");
            AssertOption(result.ParsedCommand.Command.Options["key3"], "key3", nameof(Int64), "Key3 value phone", "value3");
            AssertOption(result.ParsedCommand.Command.Options["key4"], "key4", nameof(Double), "Key4 value number", "25.36");
        }

        [DataTestMethod]
        [DataRow("pi", "orgid", "pi")]
        [DataRow("pi auth", "orgid:authid", "auth")]
        [DataRow("pi auth login", "orgid:authid:loginid", "login")]
        public async Task AliasNotConfiguredWithAliasShouldErrorAsync(string prefix, string cmdId, String cmdName)
        {
            // Disable alias
            terminalOptions.Extractor.OptionAlias = false;

            // Reset commands
            commandStore = new InMemoryCommandStore(MockCommands.AliasCommands);
            extractor = new CommandExtractor(routeParser);

            CommandExtractorContext context = new(new CommandRoute("id1", $"{prefix} -key1_alias=value1 -key2=value2 -key3_alias=value3 -key4=25.36"));

            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
                () => extractor.ExtractAsync(context),
                2,
                new[] {
                    TerminalErrors.UnsupportedOption,
                    TerminalErrors.UnsupportedOption
                },
                new[] {
                    $"The option is not supported. option=key1_alias",
                    $"The option is not supported. option=key3_alias"
                });
        }

        [TestMethod]
        public async Task BadCustomArgExtractorShouldErrorAsync()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix1 -key1=value1 -key2=value2"));
            var badExtractor = new CommandExtractor(routeParser);

            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
                () => badExtractor.ExtractAsync(context),
                2,
                new[] {
                    Error.Unexpected,
                    Error.Unexpected
                },
                new[] {
                    "The request resulted in an unexpected error. additional_info=Value cannot be null. (Parameter 'option')",
                    "The request resulted in an unexpected error. additional_info=Value cannot be null. (Parameter 'option')"
                });
        }

        [TestMethod]
        public async Task CommandDoesNotSupportArgsButUserPassedArgShouldErrorAsync()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", $"prefix4_noargs -key1=hello -key2=mello -key3 -key4=36.69"));

            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), TerminalErrors.UnsupportedOption, "The command does not support any options. command_name=name4 command_id=id4");
        }

        [DataTestMethod]
        [DataRow("pi auth", "orgid:authid", "auth")]
        [DataRow("pi auth login", "orgid:authid:loginid", "login")]
        public async Task CommandIdRegexMismatchShouldErrorAsync(string prefix, string cmdId, String cmdName)
        {
            // Disallow :
            terminalOptions.Extractor.CommandIdRegex = "^[A-Za-z0-9_-]*$";

            // Reset commands
            commandStore = new InMemoryCommandStore(MockCommands.AliasCommands);
            extractor = new CommandExtractor(routeParser);

            CommandExtractorContext context = new(new CommandRoute("id1", $"{prefix} -key1=value1 -key2=value2"));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), TerminalErrors.InvalidCommand, $"The command identifier is not valid. command_id={cmdId} regex={terminalOptions.Extractor.CommandIdRegex}");
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
            terminalOptions.Extractor.Separator = separator;
            terminalOptions.Extractor.OptionValueSeparator = separator;

            CommandExtractorContext context = new(new CommandRoute("id1", $"prefix1{separator}-key1{separator}value1{separator}-key2{separator}value2{separator}-key6{separator}-key9{separator}26.36"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            Assert.AreEqual(4, result.ParsedCommand.Command.Options.Count);
            AssertOption(result.ParsedCommand.Command.Options["key1"], "key1", nameof(String), "Key1 value text", "value1");
            AssertOption(result.ParsedCommand.Command.Options["key2"], "key2", nameof(String), "Key2 value text", "value2");
            AssertOption(result.ParsedCommand.Command.Options["key6"], "key6", nameof(Boolean), "Key6 no value", "True");
            AssertOption(result.ParsedCommand.Command.Options["key9"], "key9", nameof(Double), "Key9 value custom double", "26.36");
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
            terminalOptions.Extractor.OptionPrefix = prefix;

            CommandExtractorContext context = new(new CommandRoute("id1", $"prefix1 {prefix}key1=Test{prefix}prefix{prefix}message {prefix}key2=nospacemessage {prefix}key6 {prefix}key10=Again{prefix}with{prefix}prefix"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand);
            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            Assert.AreEqual(4, result.ParsedCommand.Command.Options.Count);
            AssertOption(result.ParsedCommand.Command.Options["key1"], "key1", nameof(String), "Key1 value text", $"Test{prefix}prefix{prefix}message");
            AssertOption(result.ParsedCommand.Command.Options["key2"], "key2", nameof(String), "Key2 value text", "nospacemessage");
            AssertOption(result.ParsedCommand.Command.Options["key6"], "key6", nameof(Boolean), "Key6 no value", "True");
            AssertOption(result.ParsedCommand.Command.Options["key10"], "key10", nameof(String), "Key10 value custom string", $"Again{prefix}with{prefix}prefix");
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
            terminalOptions.Extractor.OptionValueSeparator = separator;

            CommandExtractorContext context = new(new CommandRoute("id1", $"prefix1 -key1{separator}Test{separator}separator{separator}message -key2{separator}nosseparatormessage -key6 -key10{separator}Again{separator}with{separator}separator"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand);
            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            Assert.AreEqual(4, result.ParsedCommand.Command.Options.Count);
            AssertOption(result.ParsedCommand.Command.Options["key1"], "key1", nameof(String), "Key1 value text", $"Test{separator}separator{separator}message");
            AssertOption(result.ParsedCommand.Command.Options["key2"], "key2", nameof(String), "Key2 value text", "nosseparatormessage");
            AssertOption(result.ParsedCommand.Command.Options["key6"], "key6", nameof(Boolean), "Key6 no value", "True");
            AssertOption(result.ParsedCommand.Command.Options["key10"], "key10", nameof(String), "Key10 value custom string", $"Again{separator}with{separator}separator");
        }

        [DataTestMethod]
        [DataRow(" ")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task CommandStringShouldAllowCommandSeparatorInOptionValueAsync(string separator)
        {
            // E.g. if ' ' space is used as a command separator then the command string should allow spaces in the
            // option values
            // -> prefix1 -key=Test space message -key2=nospacemessage -key3=Again with space
            terminalOptions.Extractor.Separator = separator;

            CommandExtractorContext context = new(new CommandRoute("id1", $"prefix1{separator}-key1=Test{separator}space{separator}message{separator}-key2=nospacemessage{separator}-key6{separator}-key10=Again{separator}with{separator}space"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand);
            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            Assert.AreEqual(4, result.ParsedCommand.Command.Options.Count);
            AssertOption(result.ParsedCommand.Command.Options["key1"], "key1", nameof(String), "Key1 value text", $"Test{separator}space{separator}message");
            AssertOption(result.ParsedCommand.Command.Options["key2"], "key2", nameof(String), "Key2 value text", "nospacemessage");
            AssertOption(result.ParsedCommand.Command.Options["key6"], "key6", nameof(Boolean), "Key6 no value", "True");
            AssertOption(result.ParsedCommand.Command.Options["key10"], "key10", nameof(String), "Key10 value custom string", $"Again{separator}with{separator}space");
        }

        [DataTestMethod]
        [DataRow(" ")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task CommandStringShouldAllowMultipleSeparatorForArgumentAndOptionValueAsync(string separator)
        {
            // E.g. if ' ' space is used as a command separator then the command string should allow multiple spaces in
            // the option and spaces in option values
            // -> "prefix1 -key1=Test space message -key2=nospacemessage -key6 -key10=Again with space"
            terminalOptions.Extractor.Separator = separator;

            CommandExtractorContext context = new(new CommandRoute("id1", $"prefix1{separator}{separator}{separator}-key1=Test{separator}{separator}space{separator}{separator}{separator}{separator}message{separator}{separator}{separator}-key2=nospacemessage{separator}{separator}-key6{separator}{separator}{separator}{separator}-key10=Again{separator}with{separator}{separator}space"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand);
            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            Assert.AreEqual(4, result.ParsedCommand.Command.Options.Count);
            AssertOption(result.ParsedCommand.Command.Options["key1"], "key1", nameof(String), "Key1 value text", $"Test{separator}{separator}space{separator}{separator}{separator}{separator}message");
            AssertOption(result.ParsedCommand.Command.Options["key2"], "key2", nameof(String), "Key2 value text", "nospacemessage");
            AssertOption(result.ParsedCommand.Command.Options["key6"], "key6", nameof(Boolean), "Key6 no value", "True");
            AssertOption(result.ParsedCommand.Command.Options["key10"], "key10", nameof(String), "Key10 value custom string", $"Again{separator}with{separator}{separator}space");
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
            terminalOptions.Extractor.Separator = separator;

            CommandExtractorContext context = new(new CommandRoute("id1", $"prefix1{separator}-key1=Test space message{separator}{separator}{separator}{separator}{separator}"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand);
            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            Assert.AreEqual(1, result.ParsedCommand.Command.Options.Count);
            AssertOption(result.ParsedCommand.Command.Options["key1"], "key1", nameof(String), "Key1 value text", $"Test space message");
        }

        [TestMethod]
        [WriteDocumentation]
        public async Task DefaultOptionShouldWorkCorrectlyWithoutItsValueAsync()
        {
            // No default arg value
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix7_defaultarg"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNull(result.ParsedCommand.Command.Options);

            // No default arg value with other args
            context = new(new CommandRoute("id1", "prefix7_defaultarg -key2=hello -key6"));
            result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            Assert.AreEqual(2, result.ParsedCommand.Command.Options.Count);
            AssertOption(result.ParsedCommand.Command.Options["key2"], "key2", nameof(String), "Key2 value text", "hello");
            AssertOption(result.ParsedCommand.Command.Options["key6"], "key6", nameof(Boolean), "Key6 no value", "True");
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
        public async Task CommandWithEmptyArgsShouldNotErrorAsync()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix6_empty_args"));
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
        public async Task InvalidCommandStringWithinCommandGroupShouldErrorAsync()
        {
            // Reset commands
            commandStore = new InMemoryCommandStore(MockCommands.GroupedCommands);
            extractor = new CommandExtractor(routeParser);

            CommandExtractorContext context = new(new CommandRoute("id1", "pi auth invalid"));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), TerminalErrors.UnsupportedCommand, "The command prefix is not valid. prefix=pi auth invalid");

            context = new CommandExtractorContext(new CommandRoute("id1", "pi auth invalid -key1=value1"));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), TerminalErrors.UnsupportedCommand, "The command prefix is not valid. prefix=pi auth invalid");
        }

        [TestMethod]
        public async Task MixedSupportedAndUnsupportedArgsShouldErrorAsync()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", $"prefix1 -key1=hello -keyunsupported1 -key2=mello -key6 -key4=36.69 -keyunsupported2=value"));

            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
               () => extractor.ExtractAsync(context),
               2,
               new[] {
                    TerminalErrors.UnsupportedOption,
                    TerminalErrors.UnsupportedOption
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
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix1 -key1=value1 -key2=value2"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand.Command.Descriptor);
            Assert.AreEqual("id1", result.ParsedCommand.Command.Descriptor.Id);
            Assert.AreEqual("name1", result.ParsedCommand.Command.Descriptor.Name);
        }

        [TestMethod]
        public async Task PrefixMatchSameCommandNameShouldNotError()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "name2 -key1=value1 -key2=value2"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand.Command.Descriptor);
            Assert.AreEqual("id2", result.ParsedCommand.Command.Descriptor.Id);
            Assert.AreEqual("name2", result.ParsedCommand.Command.Descriptor.Name);
        }

        [TestMethod]
        public async Task PrefixMatchWithMultipleWordsShouldNotError()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix3 sub3 name3 -key1=value1 -key2=value2"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand.Command.Descriptor);
            Assert.AreEqual("id3", result.ParsedCommand.Command.Descriptor.Id);
            Assert.AreEqual("name3", result.ParsedCommand.Command.Descriptor.Name);
        }

        [TestMethod]
        public async Task PrefixMismatchMultipleWordsShouldError()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "invalid_prefix3 sub3 name3 -key1=value1 -key2=value2"));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), TerminalErrors.UnsupportedCommand, "The command prefix is not valid. prefix=invalid_prefix3 sub3 name3");
        }

        [TestMethod]
        public async Task PrefixMismatchShouldError()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "invalidprefix -key1=value1 -key2=value2"));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), TerminalErrors.UnsupportedCommand, "The command prefix is not valid. prefix=invalidprefix");
        }

        [TestMethod]
        public async Task TryMatchByPrefixShouldErrorIfNotFoundAsync()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "invalid prefix1"));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), TerminalErrors.UnsupportedCommand, "The command prefix is not valid. prefix=invalid prefix1");
        }

        [TestMethod]
        public async Task TryMatchByPrefixValueDelimiterSufficientPhrasesShouldErrorAsync()
        {
            // Missing name3
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix3 sub3"));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), TerminalErrors.UnsupportedCommand, "The command prefix is not valid. prefix=prefix3 sub3");
        }

        [TestMethod]
        public async Task TryMatchByPrefixWithMultiplePhrasesShouldNotErrorIfMatchedAsync()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix3 sub3 name3"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand.Command.Descriptor);
            Assert.AreEqual("id3", result.ParsedCommand.Command.Descriptor.Id);
            Assert.AreEqual("name3", result.ParsedCommand.Command.Descriptor.Name);
        }

        [TestMethod]
        public async Task UnsupportedOptionsShouldErrorMultipleAsync()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix1 -invalid_key1=value1 -invalid_key2=value2 -invalid_key3=value3"));
            await TestHelper.AssertThrowsMultiErrorExceptionAsync
            (
                () => extractor.ExtractAsync(context),
                3,
                new[] {
                    TerminalErrors.UnsupportedOption,
                    TerminalErrors.UnsupportedOption,
                    TerminalErrors.UnsupportedOption
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
            CommandExtractorContext context = new(new CommandRoute("id1", "invalid_cmd"));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), TerminalErrors.UnsupportedCommand, "The command prefix is not valid. prefix=invalid_cmd");
        }

        [DataTestMethod]
        [DataRow(" ")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task ValidCommandStringShouldNotErrorAndPopulateCorrectly(string separator)
        {
            terminalOptions.Extractor.Separator = separator;

            CommandExtractorContext context = new(new CommandRoute("id1", $"name2{separator}-key1=value1{separator}-key2=value2{separator}-key3=+1-2365985632{separator}-key4=testmail@gmail.com{separator}-key5=C:\\apps\\devop_tools\\bin\\wntx64\\i18nnotes.txt{separator}-key6{separator}-key9=33.368"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.ParsedCommand.Command.Descriptor);
            Assert.AreEqual("id2", result.ParsedCommand.Command.Descriptor.Id);
            Assert.AreEqual("name2", result.ParsedCommand.Command.Descriptor.Name);
            Assert.AreEqual("desc2", result.ParsedCommand.Command.Descriptor.Description);
            Assert.AreEqual(typeof(CommandChecker), result.ParsedCommand.Command.Descriptor.Checker);
            Assert.AreEqual(typeof(CommandRunner<CommandRunnerResult>), result.ParsedCommand.Command.Descriptor.Runner);
            Assert.IsNotNull(result.ParsedCommand.Command.Descriptor.OptionDescriptors);
            Assert.AreEqual(10, result.ParsedCommand.Command.Descriptor.OptionDescriptors.Count);
            AssertOptionDescriptor(result.ParsedCommand.Command.Descriptor.OptionDescriptors["key1"], "key1", nameof(String), "Key1 value text");
            AssertOptionDescriptor(result.ParsedCommand.Command.Descriptor.OptionDescriptors["key2"], "key2", nameof(String), "Key2 value text");
            AssertOptionDescriptor(result.ParsedCommand.Command.Descriptor.OptionDescriptors["key3"], "key3", nameof(Int64), "Key3 value phone");
            AssertOptionDescriptor(result.ParsedCommand.Command.Descriptor.OptionDescriptors["key4"], "key4", nameof(String), "Key4 value email");
            AssertOptionDescriptor(result.ParsedCommand.Command.Descriptor.OptionDescriptors["key5"], "key5", nameof(String), "Key5 value url");
            AssertOptionIdentity(result.ParsedCommand.Command.Descriptor.OptionDescriptors["key6"], "key6", nameof(Boolean), "Key6 no value");
            AssertOptionDescriptor(result.ParsedCommand.Command.Descriptor.OptionDescriptors["key7"], "key7", nameof(Int64), "Key7 value currency", new DataValidationOptionValueChecker[] { new(new OneOfAttribute("INR", "USD", "EUR")) });
            AssertOptionIdentity(result.ParsedCommand.Command.Descriptor.OptionDescriptors["key8"], "key8", nameof(Int32), "Key8 value custom int");
            AssertOptionIdentity(result.ParsedCommand.Command.Descriptor.OptionDescriptors["key9"], "key9", nameof(Double), "Key9 value custom double", new DataValidationOptionValueChecker[] { new(new RequiredAttribute()), new(new OneOfAttribute(2.36, 25.36, 3669566.36, 26.36, -36985.25, 0, -5)) });
            AssertOptionIdentity(result.ParsedCommand.Command.Descriptor.OptionDescriptors["key10"], "key10", nameof(String), "Key10 value custom string");

            // Supported options 10, user only passed 7
            Assert.IsNotNull(result.ParsedCommand);
            Assert.AreEqual("id2", result.ParsedCommand.Command.Id);
            Assert.AreEqual("name2", result.ParsedCommand.Command.Name);
            Assert.AreEqual("desc2", result.ParsedCommand.Command.Description);
            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            Assert.AreEqual(7, result.ParsedCommand.Command.Options.Count);
            AssertOption(result.ParsedCommand.Command.Options["key1"], "key1", nameof(String), "Key1 value text", "value1");
            AssertOption(result.ParsedCommand.Command.Options["key2"], "key2", nameof(String), "Key2 value text", "value2");
            AssertOption(result.ParsedCommand.Command.Options["key3"], "key3", nameof(Int64), "Key3 value phone", "+1-2365985632");
            AssertOption(result.ParsedCommand.Command.Options["key4"], "key4", nameof(String), "Key4 value email", "testmail@gmail.com");
            AssertOption(result.ParsedCommand.Command.Options["key5"], "key5", nameof(String), "Key5 value url", "C:\\apps\\devop_tools\\bin\\wntx64\\i18nnotes.txt");
            AssertOption(result.ParsedCommand.Command.Options["key6"], "key6", "Boolean", "Key6 no value", "True");
            AssertOption(result.ParsedCommand.Command.Options["key9"], "key9", "Double", "Key9 value custom double", "33.368");
        }

        [TestMethod]
        public async Task ValidCommandStringWithOptionsButNoOptionsPassedShouldNotError()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix1"));
            var result = await extractor.ExtractAsync(context);

            // Args Supported
            Assert.IsNotNull(result.ParsedCommand.Command.Descriptor);
            Assert.IsNotNull(result.ParsedCommand.Command.Descriptor.OptionDescriptors);
            Assert.AreEqual(10, result.ParsedCommand.Command.Descriptor.OptionDescriptors.Count);

            // No Args Passed
            Assert.IsNotNull(result.ParsedCommand);
            Assert.IsNull(result.ParsedCommand.Command.Options);
        }

        [TestMethod]
        public async Task ValidCommandStringWithinCommandGroupShouldNotErrorAsync()
        {
            // Reset commands
            commandStore = new InMemoryCommandStore(MockCommands.GroupedCommands);
            extractor = new CommandExtractor(routeParser);

            CommandExtractorContext context = new(new CommandRoute("id1", "pi"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand.Command.Descriptor);
            Assert.AreEqual("orgid", result.ParsedCommand.Command.Descriptor.Id);
            Assert.AreEqual("pi", result.ParsedCommand.Command.Descriptor.Name);

            context = new CommandExtractorContext(new CommandRoute("id1", "pi auth"));
            result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand.Command.Descriptor);
            Assert.AreEqual("orgid:authid", result.ParsedCommand.Command.Descriptor.Id);
            Assert.AreEqual("auth", result.ParsedCommand.Command.Descriptor.Name);

            context = new CommandExtractorContext(new CommandRoute("id1", "pi auth login"));
            result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand.Command.Descriptor);
            Assert.AreEqual("orgid:authid:loginid", result.ParsedCommand.Command.Descriptor.Id);
            Assert.AreEqual("login", result.ParsedCommand.Command.Descriptor.Name);

            context = new CommandExtractorContext(new CommandRoute("id1", "pi auth slogin"));
            result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand.Command.Descriptor);
            Assert.AreEqual("orgid:authid:sloginid", result.ParsedCommand.Command.Descriptor.Id);
            Assert.AreEqual("slogin", result.ParsedCommand.Command.Descriptor.Name);

            context = new CommandExtractorContext(new CommandRoute("id1", "pi auth slogin oidc"));
            result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand.Command.Descriptor);
            Assert.AreEqual("orgid:authid:sloginid:oidc", result.ParsedCommand.Command.Descriptor.Id);
            Assert.AreEqual("oidc", result.ParsedCommand.Command.Descriptor.Name);

            context = new CommandExtractorContext(new CommandRoute("id1", "pi auth slogin oauth"));
            result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand.Command.Descriptor);
            Assert.AreEqual("orgid:authid:sloginid:oauth", result.ParsedCommand.Command.Descriptor.Id);
            Assert.AreEqual("oauth", result.ParsedCommand.Command.Descriptor.Name);
        }

        [TestMethod]
        public async Task ValidCommandStringWithNoOptionsShouldNotErrorAsync()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix4_noargs"));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand);
            Assert.IsNull(result.ParsedCommand.Command.Options);
        }

        [TestMethod]
        public async Task ValidCommandStringWithNoSeparatorShouldErrorAsync()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "prefix1-key1=value1-key2=value2"));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), TerminalErrors.InvalidCommand, "The command separator is missing. command_string=prefix1-key1=value1-key2=value2");
        }

        [TestMethod]
        public async Task Options_Are_Extracted_Correctly_With_Multiple_ValueValueDelimiter_WithQuotes_Async()
        {
            terminalOptions = MockTerminalOptions.NewAliasOptions();
            SetupBasedOnTerminalOptions(terminalOptions);

            // No arg id
            CommandExtractorContext context = new(new CommandRoute("id9", "root9 grp9 cmd9 --key1 \"key1_value\" --key2 \"date -s '20 JUN 2023 11:06:38'\""));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            AssertOption(result.ParsedCommand.Command.Options["key1"], "key1", nameof(String), "Key1 value text", "key1_value");
            AssertOption(result.ParsedCommand.Command.Options["key2"], "key2", nameof(String), "Key2 value text", "date -s '20 JUN 2023 11:06:38'");
        }

        [TestMethod]
        public async Task Options_Are_Extracted_Correctly_With_Multiple_Alias_ValueValueDelimiter_WithQuotes_Async()
        {
            terminalOptions = MockTerminalOptions.NewAliasOptions();
            SetupBasedOnTerminalOptions(terminalOptions);

            // No arg id
            CommandExtractorContext context = new(new CommandRoute("id9", "root9 grp9 cmd9 -key1_alias \"date -s '15 JUN 2023 11:06:38'\" --key2 \"date -s '20 JUN 2023 11:06:38'\""));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            AssertOption(result.ParsedCommand.Command.Options["key1"], "key1", nameof(String), "Key1 value text", "date -s '15 JUN 2023 11:06:38'");
            AssertOption(result.ParsedCommand.Command.Options["key2"], "key2", nameof(String), "Key2 value text", "date -s '20 JUN 2023 11:06:38'");
        }

        [TestMethod]
        public async Task Options_Are_Extracted_Correctly_With_Single_Value_Has_Another_Option_ValueValueDelimiterAsync()
        {
            terminalOptions = MockTerminalOptions.NewAliasOptions();
            SetupBasedOnTerminalOptions(terminalOptions);

            // No arg id
            CommandExtractorContext context = new(new CommandRoute("id9", "root9 grp9 cmd9 --key1 \"cmd --opt1 val1 --opt2 val2 --opt3\""));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            AssertOption(result.ParsedCommand.Command.Options["key1"], "key1", nameof(String), "Key1 value text", "cmd --opt1 val1 --opt2 val2 --opt3");
        }

        [TestMethod]
        public async Task Options_Are_Extracted_Correctly_With_Single_ValueValueDelimiterAsync()
        {
            terminalOptions = MockTerminalOptions.NewAliasOptions();
            SetupBasedOnTerminalOptions(terminalOptions);

            // No arg id
            CommandExtractorContext context = new(new CommandRoute("id9", "root9 grp9 cmd9 --key1 \"key1_value\""));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            AssertOption(result.ParsedCommand.Command.Options["key1"], "key1", nameof(String), "Key1 value text", "key1_value");
        }

        [TestMethod]
        public async Task Options_Are_Extracted_Correctly_With_Multiple_ValueValueDelimiterAsync()
        {
            terminalOptions = MockTerminalOptions.NewAliasOptions();
            SetupBasedOnTerminalOptions(terminalOptions);

            // No arg id
            CommandExtractorContext context = new(new CommandRoute("id9", "root9 grp9 cmd9 --key1 \"key1_value\" --key2 \"key2_value\""));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            AssertOption(result.ParsedCommand.Command.Options["key1"], "key1", nameof(String), "Key1 value text", "key1_value");
            AssertOption(result.ParsedCommand.Command.Options["key2"], "key2", nameof(String), "Key2 value text", "key2_value");
        }

        [TestMethod]
        public async Task Options_Are_Extracted_Correctly_With_Single_Alias_ValueValueDelimiterAsync()
        {
            terminalOptions = MockTerminalOptions.NewAliasOptions();
            SetupBasedOnTerminalOptions(terminalOptions);

            // No arg id
            CommandExtractorContext context = new(new CommandRoute("id9", "root9 grp9 cmd9 -key1_alias \"key1_value\""));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            AssertOption(result.ParsedCommand.Command.Options["key1"], "key1", nameof(String), "Key1 value text", "key1_value");
        }

        [TestMethod]
        public async Task Options_Are_Extracted_Correctly_With_Multiple_Alias_ValueValueDelimiterAsync()
        {
            terminalOptions = MockTerminalOptions.NewAliasOptions();
            SetupBasedOnTerminalOptions(terminalOptions);

            // No arg id
            CommandExtractorContext context = new(new CommandRoute("id9", "root9 grp9 cmd9 -key1_alias \"key1_value\" -key5_alias \"key5_value\""));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            AssertOption(result.ParsedCommand.Command.Options["key1"], "key1", nameof(String), "Key1 value text", "key1_value");
            AssertOption(result.ParsedCommand.Command.Options["key5"], "key5", nameof(String), "Key5 value text", "key5_value");
        }

        [TestMethod]
        public async Task Options_Are_Extracted_Correctly_With_Mixed_Alias_ValueValueDelimiterAsync()
        {
            terminalOptions = MockTerminalOptions.NewAliasOptions();
            SetupBasedOnTerminalOptions(terminalOptions);

            // No arg id
            CommandExtractorContext context = new(new CommandRoute("id9", "root9 grp9 cmd9 --key1 \"key1_value\" -key5_alias \"key5_value\""));
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            AssertOption(result.ParsedCommand.Command.Options["key1"], "key1", nameof(String), "Key1 value text", "key1_value");
            AssertOption(result.ParsedCommand.Command.Options["key5"], "key5", nameof(String), "Key5 value text", "key5_value");
        }

        private void SetupBasedOnTerminalOptions(TerminalOptions terminalOptionsIpt)
        {
            textHandler = new UnicodeTextHandler();
            routeParser = new MockCommandRouteParser();
            commandStore = new InMemoryCommandStore(MockCommands.Commands);
            extractor = new CommandExtractor(routeParser);
        }

        private static void AssertOption(Option arg, string name, string dataType, string description, object value)
        {
            Assert.AreEqual(arg.Id, name);
            Assert.AreEqual(arg.DataType, dataType);
            Assert.AreEqual(arg.Description, description);
            Assert.AreEqual(arg.Value, value);
        }

        private static void AssertOptionDescriptor(OptionDescriptor arg, string name, string dataType, string? description = null, DataValidationOptionValueChecker[]? supportedValues = null)
        {
            Assert.AreEqual(arg.Id, name);
            Assert.AreEqual(arg.DataType, dataType);
            Assert.AreEqual(arg.Description, description);

            DataValidationOptionValueChecker[]? expectedCheckers = arg.ValueCheckers?.Cast<DataValidationOptionValueChecker>().ToArray();
            CollectionAssert.AreEquivalent(expectedCheckers, supportedValues);
        }

        private void AssertOptionIdentity(OptionDescriptor arg, string name, string dataType, string? description = null, DataValidationOptionValueChecker[]? supportedValues = null)
        {
            Assert.AreEqual(arg.Id, name);
            Assert.AreEqual(arg.DataType, dataType);
            Assert.AreEqual(arg.Description, description);
            CollectionAssert.AreEquivalent(arg.ValueCheckers?.Cast<DataValidationOptionValueChecker>().ToArray(), supportedValues);
        }

        private ICommandStoreHandler commandStore = null!;
        private ICommandRouteParser routeParser = null!;
        private CommandExtractor extractor = null!;
        private TerminalOptions terminalOptions = null!;
        private ITextHandler textHandler = null!;
    }
}