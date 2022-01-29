/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Extractors.Mocks;
using PerpetualIntelligence.Cli.Commands.Providers;
using PerpetualIntelligence.Cli.Commands.Runners;
using PerpetualIntelligence.Cli.Commands.Stores;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Cli.Stores.InMemory;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Shared.Attributes.Validation;
using PerpetualIntelligence.Shared.Infrastructure;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    [TestClass]
    public class SeparatorCommandExtractorTests : LogTest
    {
        public SeparatorCommandExtractorTests() : base(TestLogger.Create<SeparatorCommandExtractorTests>())
        {
        }

        [TestMethod]
        public async Task ArgumentPrefixCannotBeNullOrWhitespaceAsync()
        {
            options.Extractor.ArgumentPrefix = null;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(new CommandExtractorContext("test")), Errors.InvalidConfiguration, $"The argument prefix cannot be null or whitespace.");

            options.Extractor.ArgumentPrefix = "";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(new CommandExtractorContext("test")), Errors.InvalidConfiguration, $"The argument prefix cannot be null or whitespace.");

            options.Extractor.ArgumentPrefix = "   ";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(new CommandExtractorContext("test")), Errors.InvalidConfiguration, $"The argument prefix cannot be null or whitespace.");
        }

        [DataTestMethod]
        [DataRow("@")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task ArgumentSeparatorAndArgumentPrefixCannotBeSameAsync(string separator)
        {
            // Make sure command separator is different so we can fail for argument separator below.
            options.Extractor.Separator = "-";

            options.Extractor.ArgumentSeparator = separator;
            options.Extractor.ArgumentPrefix = separator;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(new CommandExtractorContext("test")), Errors.InvalidConfiguration, $"The argument separator and argument prefix cannot be same. separator={separator}");
        }

        [TestMethod]
        public async Task ArgumentSeparatorCannotBeNullOrWhitespaceAsync()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            options.Extractor.ArgumentSeparator = null;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(new CommandExtractorContext("test")), Errors.InvalidConfiguration, $"The argument separator cannot be null or whitespace.");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            options.Extractor.ArgumentSeparator = "";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(new CommandExtractorContext("test")), Errors.InvalidConfiguration, $"The argument separator cannot be null or whitespace.");

            options.Extractor.ArgumentSeparator = "   ";
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(new CommandExtractorContext("test")), Errors.InvalidConfiguration, $"The argument separator cannot be null or whitespace.");
        }

        [TestMethod]
        public async Task BadCustomArgExtractorShouldErrorAsync()
        {
            CommandExtractorContext context = new CommandExtractorContext("prefix1 -key1=value1 -key2=value2");
            var badExtractor = new SeparatorCommandExtractor(commands, new MockBadArgumentExtractor(), options, TestLogger.Create<SeparatorCommandExtractor>());

            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
                () => badExtractor.ExtractAsync(context),
                2,
                new[] {
                    Error.Unexpected,
                    Error.Unexpected
                },
                new[] {
                    "The request resulted in an unexpected error. additonal_info=Value cannot be null. (Parameter 'argument')",
                    "The request resulted in an unexpected error. additonal_info=Value cannot be null. (Parameter 'argument')"
                });
        }

        [TestMethod]
        public async Task CommandDoesNotSupportArgsButUserPassedArgShouldErrorAsync()
        {
            CommandExtractorContext context = new($"prefix4_noargs -key1=hello -key2=mello -key3 -key4=36.69");

            await TestHelper.AssertThrowsMultiErrorExceptionAsync(
               () => extractor.ExtractAsync(context),
               4,
               new[] {
                    Errors.UnsupportedArgument,
                    Errors.UnsupportedArgument,
                    Errors.UnsupportedArgument,
                    Errors.UnsupportedArgument
               },
               new[] {
                    "The argument is not supported. command_name=name4 command_id=id4 argument=key1",
                    "The argument is not supported. command_name=name4 command_id=id4 argument=key2",
                    "The argument is not supported. command_name=name4 command_id=id4 argument=key3",
                    "The argument is not supported. command_name=name4 command_id=id4 argument=key4"
               });
        }

        [DataTestMethod]
        [DataRow(" ")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task CommandSeparatorAndArgumentPrefixCannotBeSameAsync(string separator)
        {
            options.Extractor.Separator = separator;
            options.Extractor.ArgumentPrefix = separator;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(new CommandExtractorContext("test")), Errors.InvalidConfiguration, $"The command separator and argument prefix cannot be same. separator={separator}");
        }

        [DataTestMethod]
        [DataRow(" ")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task CommandSeparatorAndArgumentSeparatorCannotBeSameAsync(string separator)
        {
            options.Extractor.Separator = separator;
            options.Extractor.ArgumentSeparator = separator;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(new CommandExtractorContext("test")), Errors.InvalidConfiguration, $"The command separator and argument separator cannot be same. separator={separator}");
        }

        [TestMethod]
        public async Task CommandSeparatorCannotBeNullAsync()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            options.Extractor.Separator = null;
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(new CommandExtractorContext("test")), Errors.InvalidConfiguration, $"The command separator is null or not configured.");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [DataTestMethod]
        [DataRow("@")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task CommandStringShouldAllowArgumentPrefixInArgumentValueAsync(string prefix)
        {
            // E.g. if ' ' space is used as a command separator then the command string should allow spaces in the
            // argument values
            // -> prefix1 -key=Test-prefix-message -key2=nosprefixmessage -key3=again-with-prefix
            options.Extractor.ArgumentPrefix = prefix;

            CommandExtractorContext context = new CommandExtractorContext($"prefix1 {prefix}key1=Test{prefix}prefix{prefix}message {prefix}key2=nospacemessage {prefix}key6 {prefix}key10=Again{prefix}with{prefix}prefix");
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command);
            Assert.IsNotNull(result.Command.Arguments);
            Assert.AreEqual(4, result.Command.Arguments.Count);
            AssertArgument(result.Command.Arguments[0], "key1", DataType.Text, "Key1 value text", $"Test{prefix}prefix{prefix}message");
            AssertArgument(result.Command.Arguments[1], "key2", DataType.Text, "Key2 value text", "nospacemessage");
            AssertArgument(result.Command.Arguments[2], "key6", nameof(Boolean), "Key6 no value", true);
            AssertArgument(result.Command.Arguments[3], "key10", nameof(String), "Key10 value custom string", $"Again{prefix}with{prefix}prefix");
        }

        [DataTestMethod]
        [DataRow("@")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task CommandStringShouldAllowArgumentSeparatorInArgumentValueAsync(string separator)
        {
            // E.g. if ' ' space is used as a command separator then the command string should allow spaces in the
            // argument values
            // -> prefix1 -key=Test=separator=message -key2=nosseparatormessage -key3=again=with=separator
            options.Extractor.ArgumentSeparator = separator;

            CommandExtractorContext context = new CommandExtractorContext($"prefix1 -key1{separator}Test{separator}separator{separator}message -key2{separator}nosseparatormessage -key6 -key10{separator}Again{separator}with{separator}separator");
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command);
            Assert.IsNotNull(result.Command.Arguments);
            Assert.AreEqual(4, result.Command.Arguments.Count);
            AssertArgument(result.Command.Arguments[0], "key1", DataType.Text, "Key1 value text", $"Test{separator}separator{separator}message");
            AssertArgument(result.Command.Arguments[1], "key2", DataType.Text, "Key2 value text", "nosseparatormessage");
            AssertArgument(result.Command.Arguments[2], "key6", nameof(Boolean), "Key6 no value", true);
            AssertArgument(result.Command.Arguments[3], "key10", nameof(String), "Key10 value custom string", $"Again{separator}with{separator}separator");
        }

        [DataTestMethod]
        [DataRow(" ")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task CommandStringShouldAllowCommandSeparatorInArgumentValueAsync(string seperator)
        {
            // E.g. if ' ' space is used as a command separator then the command string should allow spaces in the
            // argument values
            // -> prefix1 -key=Test space message -key2=nospacemessage -key3=again with space
            options.Extractor.Separator = seperator;

            CommandExtractorContext context = new CommandExtractorContext($"prefix1{seperator}-key1=Test{seperator}space{seperator}message{seperator}-key2=nospacemessage{seperator}-key6{seperator}-key10=Again{seperator}with{seperator}space");
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command);
            Assert.IsNotNull(result.Command.Arguments);
            Assert.AreEqual(4, result.Command.Arguments.Count);
            AssertArgument(result.Command.Arguments[0], "key1", DataType.Text, "Key1 value text", $"Test{seperator}space{seperator}message");
            AssertArgument(result.Command.Arguments[1], "key2", DataType.Text, "Key2 value text", "nospacemessage");
            AssertArgument(result.Command.Arguments[2], "key6", nameof(Boolean), "Key6 no value", true);
            AssertArgument(result.Command.Arguments[3], "key10", nameof(String), "Key10 value custom string", $"Again{seperator}with{seperator}space");
        }

        [TestMethod]
        public async Task DefaultValueConfiguredCommandWithNoArgsShouldNotErrorAsync()
        {
            options.Extractor.ArgumentDefaultValue = true;

            CommandExtractorContext context = new("prefix4_noargs");
            var result = await extractor.ExtractAsync(context);

            Assert.IsNull(result.Command.Arguments);
        }

        [TestMethod]
        public async Task DefaultValueConfiguredCommandWithEmptyArgsShouldNotErrorAsync()
        {
            options.Extractor.ArgumentDefaultValue = true;

            CommandExtractorContext context = new("prefix6_empty_args");
            var result = await extractor.ExtractAsync(context);

            Assert.IsNull(result.Command.Arguments);
        }

        [TestMethod]
        public async Task DefaultValueNotConfiguredCommandWithNoArgsShouldNotErrorAsync()
        {
            options.Extractor.ArgumentDefaultValue = false;

            CommandExtractorContext context = new("prefix4_noargs");
            var result = await extractor.ExtractAsync(context);

            Assert.IsNull(result.Command.Arguments);
        }

        [TestMethod]
        public async Task DefaultValuesConfiguredButProviderNotConfiguredShouldThrow()
        {
            options.Extractor.ArgumentDefaultValue = true;

            CommandExtractorContext context = new("prefix5_default");
            SeparatorCommandExtractor noProviderExtrator = new(commands, argExtractor, options, TestLogger.Create<SeparatorCommandExtractor>(), argumentDefaultValueProvider: null);

            await TestHelper.AssertThrowsErrorExceptionAsync(() => noProviderExtrator.ExtractAsync(context), Errors.InvalidConfiguration, "The argument default value provider is missing in the service collection. provider_type=PerpetualIntelligence.Cli.Commands.Providers.IArgumentDefaultValueProvider");
        }

        [TestMethod]
        public async Task DefaultValuesConfiguredButUnspecifiedRequiredValuesShouldNotError()
        {
            options.Extractor.ArgumentDefaultValue = true;

            // This is just extracting no checking
            CommandExtractorContext context = new("prefix5_default");
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.Command.Arguments);
            Assert.AreEqual(4, result.Command.Arguments.Count);
            Assert.AreEqual("44444444444", result.Command.Arguments[0].Value);
            Assert.AreEqual(false, result.Command.Arguments[1].Value);
            Assert.AreEqual(25.36, result.Command.Arguments[2].Value);
            Assert.AreEqual("mello default", result.Command.Arguments[3].Value);
        }

        [TestMethod]
        public async Task DefaultValuesConfiguredButUnspecifiedRequiredValuesShouldNotOverrideUserValues()
        {
            options.Extractor.ArgumentDefaultValue = true;

            // This is just extracting no checking
            CommandExtractorContext context = new("prefix5_default -key6 -key10=user value");
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.Command.Arguments);
            Assert.AreEqual(4, result.Command.Arguments.Count);

            // Argument values are processed sequentially and default values are added at the end User values
            Assert.AreEqual("key6", result.Command.Arguments[0].Id);
            Assert.AreEqual(true, result.Command.Arguments[0].Value);
            Assert.AreEqual("key10", result.Command.Arguments[1].Id);
            Assert.AreEqual("user value", result.Command.Arguments[1].Value);

            // Default value are added in the end
            Assert.AreEqual("key3", result.Command.Arguments[2].Id);
            Assert.AreEqual("44444444444", result.Command.Arguments[2].Value);
            Assert.AreEqual("key9", result.Command.Arguments[3].Id);
            Assert.AreEqual(25.36, result.Command.Arguments[3].Value);
        }

        [TestMethod]
        public async Task DefaultValuesConfiguredButUnspecifiedRequiredValuesShouldNotPopulateIfDisabled()
        {
            options.Extractor.ArgumentDefaultValue = false;

            // This is just extracting no checking
            CommandExtractorContext context = new("prefix5_default");
            var result = await extractor.ExtractAsync(context);

            Assert.IsNull(result.Command.Arguments);
        }

        [TestMethod]
        public async Task DefaultValuesDisabledButProviderNotConfiguredShouldNotThrow()
        {
            options.Extractor.ArgumentDefaultValue = false;

            CommandExtractorContext context = new("prefix5_default");
            SeparatorCommandExtractor noProviderExtrator = new(commands, argExtractor, options, TestLogger.Create<SeparatorCommandExtractor>(), argumentDefaultValueProvider: null);
            await noProviderExtrator.ExtractAsync(context);
        }

        [TestMethod]
        public async Task InvalidCommandStringWithinCommandGroupShouldFailAsync()
        {
            // Reset commands
            commands = new InMemoryCommandDescriptorStore(MockCommands.GroupedCommands);
            extractor = new SeparatorCommandExtractor(commands, argExtractor, options, TestLogger.Create<SeparatorCommandExtractor>());

            CommandExtractorContext context = new CommandExtractorContext("pi auth invalid");
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedCommand, "The command prefix is not valid. prefix=pi auth invalid");

            context = new CommandExtractorContext("pi auth invalid -key1=value1");
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedCommand, "The command prefix is not valid. prefix=pi auth invalid");
        }

        [TestMethod]
        public void NullOrEmptyCommandStringShouldThrow()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            TestHelper.AssertThrowsWithMessage<ArgumentException>(() => new CommandExtractorContext(null), "'commandString' cannot be null or whitespace. (Parameter 'commandString')");
            TestHelper.AssertThrowsWithMessage<ArgumentException>(() => new CommandExtractorContext("   "), "'commandString' cannot be null or whitespace. (Parameter 'commandString')");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [TestMethod]
        public async Task PrefixMatchButDifferentCommandNameShouldNotError()
        {
            CommandExtractorContext context = new CommandExtractorContext("prefix1 -key1=value1 -key2=value2");
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.CommandDescriptor);
            Assert.AreEqual("id1", result.CommandDescriptor.Id);
            Assert.AreEqual("name1", result.CommandDescriptor.Name);
            Assert.AreEqual("prefix1", result.CommandDescriptor.Prefix);
        }

        [TestMethod]
        public async Task PrefixMatchSameCommandNameShouldNotError()
        {
            CommandExtractorContext context = new CommandExtractorContext("name2 -key1=value1 -key2=value2");
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.CommandDescriptor);
            Assert.AreEqual("id2", result.CommandDescriptor.Id);
            Assert.AreEqual("name2", result.CommandDescriptor.Name);
            Assert.AreEqual("name2", result.CommandDescriptor.Prefix);
        }

        [TestMethod]
        public async Task PrefixMatchWithMultipleWordsShouldNotError()
        {
            CommandExtractorContext context = new("prefix3 sub3 name3 -key1=value1 -key2=value2");
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.CommandDescriptor);
            Assert.AreEqual("id3", result.CommandDescriptor.Id);
            Assert.AreEqual("name3", result.CommandDescriptor.Name);
            Assert.AreEqual("prefix3 sub3 name3", result.CommandDescriptor.Prefix);
        }

        [TestMethod]
        public async Task PrefixMismatchMultipleWordsShouldError()
        {
            CommandExtractorContext context = new("invalid_prefix3 sub3 name3 -key1=value1 -key2=value2");
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedCommand, "The command prefix is not valid. prefix=invalid_prefix3 sub3 name3");
        }

        [TestMethod]
        public async Task PrefixMismatchShouldError()
        {
            CommandExtractorContext context = new("invalidprefix -key1=value1 -key2=value2");
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedCommand, "The command prefix is not valid. prefix=invalidprefix");
        }

        [TestMethod]
        public async Task TryMatchByPrefixShouldErrorIfNotFoundAsync()
        {
            CommandExtractorContext context = new("invalid prefix1");
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedCommand, "The command prefix is not valid. prefix=invalid prefix1");
        }

        [TestMethod]
        public async Task TryMatchByPrefixWithInsufficientPharasesShouldErrorAsync()
        {
            // Missing name3
            CommandExtractorContext context = new("prefix3 sub3");
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedCommand, "The command prefix is not valid. prefix=prefix3 sub3");
        }

        [TestMethod]
        public async Task TryMatchByPrefixWithMultiplePharasesShouldNotErrorIfMatchedAsync()
        {
            CommandExtractorContext context = new CommandExtractorContext("prefix3 sub3 name3");
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.CommandDescriptor);
            Assert.AreEqual("id3", result.CommandDescriptor.Id);
            Assert.AreEqual("name3", result.CommandDescriptor.Name);
            Assert.AreEqual("prefix3 sub3 name3", result.CommandDescriptor.Prefix);
        }

        [TestMethod]
        public async Task UnsupportedArgumentsShouldErrorMultipleAsync()
        {
            CommandExtractorContext context = new CommandExtractorContext("prefix1 -invalid_key1=value1 -invalid_key2=value2 -invalid_key3=value3");
            await TestHelper.AssertThrowsMultiErrorExceptionAsync
            (
                () => extractor.ExtractAsync(context),
                3,
                new[] {
                    Errors.UnsupportedArgument,
                    Errors.UnsupportedArgument,
                    Errors.UnsupportedArgument
                },
                new[] {
                    "The argument is not supported. command_name=name1 command_id=id1 argument=invalid_key1",
                    "The argument is not supported. command_name=name1 command_id=id1 argument=invalid_key2",
                    "The argument is not supported. command_name=name1 command_id=id1 argument=invalid_key3"
                }
            );
        }

        [TestMethod]
        public async Task UnsupportedCommandShouldErrorAsync()
        {
            CommandExtractorContext context = new CommandExtractorContext("invalid_cmd");
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
            options.Extractor.Separator = seperator;

            CommandExtractorContext context = new CommandExtractorContext($"name2{seperator}-key1=value1{seperator}-key2=value2{seperator}-key3=+1-2365985632{seperator}-key4=testmail@gmail.com{seperator}-key5=C:\\apps\\devop_tools\\bin\\wntx64\\i18nnotes.txt{seperator}-key6{seperator}-key9=33.368");
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.CommandDescriptor);
            Assert.AreEqual("id2", result.CommandDescriptor.Id);
            Assert.AreEqual("name2", result.CommandDescriptor.Name);
            Assert.AreEqual("name2", result.CommandDescriptor.Prefix);
            Assert.AreEqual("desc2", result.CommandDescriptor.Description);
            Assert.AreEqual(typeof(CommandChecker), result.CommandDescriptor.Checker);
            Assert.AreEqual(typeof(CommandRunner), result.CommandDescriptor.Runner);
            Assert.IsNotNull(result.CommandDescriptor.ArgumentDescriptors);
            Assert.AreEqual(10, result.CommandDescriptor.ArgumentDescriptors.Count);
            AssertArgumentIdentity(result.CommandDescriptor.ArgumentDescriptors[0], "key1", DataType.Text, "Key1 value text");
            AssertArgumentIdentity(result.CommandDescriptor.ArgumentDescriptors[1], "key2", DataType.Text, "Key2 value text", new ValidationAttribute[] { new RequiredAttribute() });
            AssertArgumentIdentity(result.CommandDescriptor.ArgumentDescriptors[2], "key3", DataType.PhoneNumber, "Key3 value phone");
            AssertArgumentIdentity(result.CommandDescriptor.ArgumentDescriptors[3], "key4", DataType.EmailAddress, "Key4 value email");
            AssertArgumentIdentity(result.CommandDescriptor.ArgumentDescriptors[4], "key5", DataType.Url, "Key5 value url");
            AssertArgumentIdentity(result.CommandDescriptor.ArgumentDescriptors[5], "key6", nameof(Boolean), "Key6 no value");
            AssertArgumentIdentity(result.CommandDescriptor.ArgumentDescriptors[6], "key7", DataType.Currency, "Key7 value currency", new ValidationAttribute[] { new OneOfAttribute("INR", "USD", "EUR"), new RequiredAttribute() });
            AssertArgumentIdentity(result.CommandDescriptor.ArgumentDescriptors[7], "key8", nameof(Int32), "Key8 value custom int");
            AssertArgumentIdentity(result.CommandDescriptor.ArgumentDescriptors[8], "key9", nameof(Double), "Key9 value custom double", new ValidationAttribute[] { new RequiredAttribute(), new OneOfAttribute(2.36, 25.36, 3669566.36, 26.36, -36985.25, 0, -5) });
            AssertArgumentIdentity(result.CommandDescriptor.ArgumentDescriptors[9], "key10", nameof(String), "Key10 value custom string", new ValidationAttribute[] { new RequiredAttribute() });

            // Supported arguments 10, user only passed 7
            Assert.IsNotNull(result.Command);
            Assert.AreEqual("id2", result.Command.Id);
            Assert.AreEqual("name2", result.Command.Name);
            Assert.AreEqual("desc2", result.Command.Description);
            Assert.IsNotNull(result.Command.Arguments);
            Assert.AreEqual(7, result.Command.Arguments.Count);
            AssertArgument(result.Command.Arguments[0], "key1", DataType.Text, "Key1 value text", "value1");
            AssertArgument(result.Command.Arguments[1], "key2", DataType.Text, "Key2 value text", "value2");
            AssertArgument(result.Command.Arguments[2], "key3", DataType.PhoneNumber, "Key3 value phone", "+1-2365985632");
            AssertArgument(result.Command.Arguments[3], "key4", DataType.EmailAddress, "Key4 value email", "testmail@gmail.com");
            AssertArgument(result.Command.Arguments[4], "key5", DataType.Url, "Key5 value url", "C:\\apps\\devop_tools\\bin\\wntx64\\i18nnotes.txt");
            AssertArgument(result.Command.Arguments[5], "key6", "Boolean", "Key6 no value", true);
            AssertArgument(result.Command.Arguments[6], "key9", "Double", "Key9 value custom double", "33.368");
        }

        [TestMethod]
        public async Task ValidCommandStringWithArgumentsButNoArgumentsPassedShouldNotFail()
        {
            CommandExtractorContext context = new CommandExtractorContext("prefix1");
            var result = await extractor.ExtractAsync(context);

            // Args Supported
            Assert.IsNotNull(result.CommandDescriptor);
            Assert.IsNotNull(result.CommandDescriptor.ArgumentDescriptors);
            Assert.AreEqual(10, result.CommandDescriptor.ArgumentDescriptors.Count);

            // No Args Passed
            Assert.IsNotNull(result.Command);
            Assert.IsNull(result.Command.Arguments);
        }

        [TestMethod]
        public async Task ValidCommandStringWithinCommandGroupShouldNotFailAsync()
        {
            // Reset commands
            commands = new InMemoryCommandDescriptorStore(MockCommands.GroupedCommands);
            extractor = new SeparatorCommandExtractor(commands, argExtractor, options, TestLogger.Create<SeparatorCommandExtractor>());

            CommandExtractorContext context = new CommandExtractorContext("pi");
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.CommandDescriptor);
            Assert.AreEqual("orgid", result.CommandDescriptor.Id);
            Assert.AreEqual("pi", result.CommandDescriptor.Name);

            context = new CommandExtractorContext("pi auth");
            result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.CommandDescriptor);
            Assert.AreEqual("orgid:authid", result.CommandDescriptor.Id);
            Assert.AreEqual("auth", result.CommandDescriptor.Name);

            context = new CommandExtractorContext("pi auth login");
            result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.CommandDescriptor);
            Assert.AreEqual("orgid:authid:loginid", result.CommandDescriptor.Id);
            Assert.AreEqual("login", result.CommandDescriptor.Name);

            context = new CommandExtractorContext("pi auth slogin");
            result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.CommandDescriptor);
            Assert.AreEqual("orgid:authid:sloginid", result.CommandDescriptor.Id);
            Assert.AreEqual("slogin", result.CommandDescriptor.Name);

            context = new CommandExtractorContext("pi auth slogin oidc");
            result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.CommandDescriptor);
            Assert.AreEqual("orgid:authid:sloginid:oidc", result.CommandDescriptor.Id);
            Assert.AreEqual("oidc", result.CommandDescriptor.Name);

            context = new CommandExtractorContext("pi auth slogin oauth");
            result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.CommandDescriptor);
            Assert.AreEqual("orgid:authid:sloginid:oauth", result.CommandDescriptor.Id);
            Assert.AreEqual("oauth", result.CommandDescriptor.Name);
        }

        [TestMethod]
        public async Task ValidCommandStringWithNoArgumentsShouldNotErrorAsync()
        {
            CommandExtractorContext context = new CommandExtractorContext("prefix4_noargs");
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Command);
            Assert.IsNull(result.Command.Arguments);
        }

        [TestMethod]
        public async Task ValidCommandStringWithNoSeparatorShouldFailAsync()
        {
            CommandExtractorContext context = new CommandExtractorContext("prefix1-key1=value1-key2=value2");
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.InvalidCommand, "The command separator is missing. command_string=prefix1-key1=value1-key2=value2");
        }

        protected override void OnTestInitialize()
        {
            options = MockCliOptions.New();
            commands = new InMemoryCommandDescriptorStore(MockCommands.Commands);
            argExtractor = new SeparatorArgumentExtractor(options, TestLogger.Create<SeparatorArgumentExtractor>());
            defualtProvider = new ArgumentDefaultValueProvider(options, TestLogger.Create<ArgumentDefaultValueProvider>());
            extractor = new SeparatorCommandExtractor(commands, argExtractor, options, TestLogger.Create<SeparatorCommandExtractor>(), defualtProvider);
        }

        private void AssertArgument(Argument arg, string name, string customDataType, string description, object value)
        {
            Assert.AreEqual(arg.Id, name);
            Assert.AreEqual(arg.DataType, DataType.Custom);
            Assert.AreEqual(arg.CustomDataType, customDataType);
            Assert.AreEqual(arg.Description, description);
            Assert.AreEqual(arg.Value, value);
        }

        private void AssertArgument(Argument arg, string name, DataType dataType, string description, object value)
        {
            Assert.AreEqual(arg.Id, name);
            Assert.AreEqual(arg.DataType, dataType);
            Assert.IsNull(arg.CustomDataType);
            Assert.AreEqual(arg.Description, description);
            Assert.AreEqual(arg.Value, value);
        }

        private void AssertArgumentIdentity(ArgumentDescriptor arg, string name, DataType dataType, string? description = null, ValidationAttribute[]? supportedValues = null)
        {
            Assert.AreEqual(arg.Id, name);
            Assert.AreEqual(arg.DataType, dataType);
            Assert.IsNull(arg.CustomDataType);
            Assert.AreEqual(arg.Description, description);
            CollectionAssert.AreEquivalent(arg.ValidationAttributes?.ToArray(), supportedValues);
        }

        private void AssertArgumentIdentity(ArgumentDescriptor arg, string name, string customDataType, string? description = null, ValidationAttribute[]? supportedValues = null)
        {
            Assert.AreEqual(arg.Id, name);
            Assert.AreEqual(arg.DataType, DataType.Custom);
            Assert.AreEqual(arg.CustomDataType, customDataType);
            Assert.AreEqual(arg.Description, description);
            CollectionAssert.AreEquivalent(arg.ValidationAttributes?.ToArray(), supportedValues);
        }

        private SeparatorArgumentExtractor argExtractor = null!;
        private ICommandDescriptorStore commands = null!;
        private IArgumentDefaultValueProvider defualtProvider = null!;
        private SeparatorCommandExtractor extractor = null!;
        private CliOptions options = null!;
    }
}
