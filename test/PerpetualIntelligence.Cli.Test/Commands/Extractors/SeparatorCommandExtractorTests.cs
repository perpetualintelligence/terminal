/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Extractors.Mocks;
using PerpetualIntelligence.Cli.Commands.Runners;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Cli.Stores.InMemory;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    [TestClass]
    public class SeparatorCommandExtractorTests : OneImlxLogTest
    {
        public SeparatorCommandExtractorTests() : base(TestLogger.Create<SeparatorCommandExtractorTests>())
        {
        }

        [TestMethod]
        public async Task ArgumentPrefixCannotBeNullOrWhitespaceAsync()
        {
            options.Extractor.ArgumentPrefix = null;
            var result = await extractor.ExtractAsync(new CommandExtractorContext("test"));
            Assert.IsTrue(result.IsError);
            TestHelper.AssertOneImlxError(result, Errors.InvalidConfiguration, $"The argument prefix cannot be null or whitespace.");

            options.Extractor.ArgumentPrefix = "";
            result = await extractor.ExtractAsync(new CommandExtractorContext("test"));
            Assert.IsTrue(result.IsError);
            TestHelper.AssertOneImlxError(result, Errors.InvalidConfiguration, $"The argument prefix cannot be null or whitespace.");

            options.Extractor.ArgumentPrefix = "   ";
            result = await extractor.ExtractAsync(new CommandExtractorContext("test"));
            Assert.IsTrue(result.IsError);
            TestHelper.AssertOneImlxError(result, Errors.InvalidConfiguration, $"The argument prefix cannot be null or whitespace.");
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
            var result = await extractor.ExtractAsync(new CommandExtractorContext("test"));
            Assert.IsTrue(result.IsError);
            TestHelper.AssertOneImlxError(result, Errors.InvalidConfiguration, $"The argument separator and argument prefix cannot be same. separator={separator}");
        }

        [TestMethod]
        public async Task ArgumentSeparatorCannotBeNullOrWhitespaceAsync()
        {
            options.Extractor.ArgumentSeparator = null;
            var result = await extractor.ExtractAsync(new CommandExtractorContext("test"));
            Assert.IsTrue(result.IsError);
            TestHelper.AssertOneImlxError(result, Errors.InvalidConfiguration, $"The argument separator cannot be null or whitespace.");

            options.Extractor.ArgumentSeparator = "";
            result = await extractor.ExtractAsync(new CommandExtractorContext("test"));
            Assert.IsTrue(result.IsError);
            TestHelper.AssertOneImlxError(result, Errors.InvalidConfiguration, $"The argument separator cannot be null or whitespace.");

            options.Extractor.ArgumentSeparator = "   ";
            result = await extractor.ExtractAsync(new CommandExtractorContext("test"));
            Assert.IsTrue(result.IsError);
            TestHelper.AssertOneImlxError(result, Errors.InvalidConfiguration, $"The argument separator cannot be null or whitespace.");
        }

        [TestMethod]
        public async Task BadCustomArgExtractorShouldErrorAsync()
        {
            CommandExtractorContext context = new CommandExtractorContext("prefix1 -key1=value1 -key2=value2");
            var badExtractor = new SeparatorCommandExtractor(commands, new MockBadArgumentExtractor(), options, TestLogger.Create<SeparatorCommandExtractor>());
            var result = await badExtractor.ExtractAsync(context);
            Assert.IsTrue(result.IsError);
            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(2, result.Errors.Length);
            TestHelper.AssertOneImlxError(result.Errors[0], Errors.InvalidArgument, "The argument string did not return an error or extract the argument. argument_string=-key1=value1");
            TestHelper.AssertOneImlxError(result.Errors[1], Errors.InvalidArgument, "The argument string did not return an error or extract the argument. argument_string=-key2=value2");
        }

        [TestMethod]
        public async Task BadCustomCustomerIdentityStoreShouldErrorAsync()
        {
            CommandExtractorContext context = new CommandExtractorContext("prefix1 -key1=value1 -key2=value2");
            var badExtractor = new SeparatorCommandExtractor(new MockBadCommandsIdentityStore(), argExtractor, options, TestLogger.Create<SeparatorCommandExtractor>());
            var result = await badExtractor.ExtractAsync(context);
            TestHelper.AssertOneImlxError(result, Errors.InvalidCommand, "The command string did not return an error or match the command prefix. command_string=prefix1 -key1=value1 -key2=value2");
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
            var result = await extractor.ExtractAsync(new CommandExtractorContext("test"));
            Assert.IsTrue(result.IsError);
            TestHelper.AssertOneImlxError(result, Errors.InvalidConfiguration, $"The command separator and argument prefix cannot be same. separator={separator}");
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
            var result = await extractor.ExtractAsync(new CommandExtractorContext("test"));
            Assert.IsTrue(result.IsError);
            TestHelper.AssertOneImlxError(result, Errors.InvalidConfiguration, $"The command separator and argument separator cannot be same. separator={separator}");
        }

        [TestMethod]
        public async Task CommandSeparatorCannotBeNullAsync()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            options.Extractor.Separator = null;
            var result = await extractor.ExtractAsync(new CommandExtractorContext("test"));
            Assert.IsTrue(result.IsError);
            TestHelper.AssertOneImlxError(result, Errors.InvalidConfiguration, $"The command separator is null or not configured.");
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
            Assert.IsFalse(result.IsError);
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
            Assert.IsFalse(result.IsError);
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
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Command);
            Assert.IsNotNull(result.Command.Arguments);
            Assert.AreEqual(4, result.Command.Arguments.Count);
            AssertArgument(result.Command.Arguments[0], "key1", DataType.Text, "Key1 value text", $"Test{seperator}space{seperator}message");
            AssertArgument(result.Command.Arguments[1], "key2", DataType.Text, "Key2 value text", "nospacemessage");
            AssertArgument(result.Command.Arguments[2], "key6", nameof(Boolean), "Key6 no value", true);
            AssertArgument(result.Command.Arguments[3], "key10", nameof(String), "Key10 value custom string", $"Again{seperator}with{seperator}space");
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
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.CommandIdentity);
            Assert.AreEqual("id1", result.CommandIdentity.Id);
            Assert.AreEqual("name1", result.CommandIdentity.Name);
            Assert.AreEqual("prefix1", result.CommandIdentity.Prefix);
        }

        [TestMethod]
        public async Task PrefixMatchSameCommandNameShouldNotError()
        {
            CommandExtractorContext context = new CommandExtractorContext("name2 -key1=value1 -key2=value2");
            var result = await extractor.ExtractAsync(context);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.CommandIdentity);
            Assert.AreEqual("id2", result.CommandIdentity.Id);
            Assert.AreEqual("name2", result.CommandIdentity.Name);
            Assert.AreEqual("name2", result.CommandIdentity.Prefix);
        }

        [TestMethod]
        public async Task PrefixMatchWithMultipleWordsShouldNotError()
        {
            CommandExtractorContext context = new CommandExtractorContext("prefix3 sub3 name3 -key1=value1 -key2=value2");
            var result = await extractor.ExtractAsync(context);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.CommandIdentity);
            Assert.AreEqual("id3", result.CommandIdentity.Id);
            Assert.AreEqual("name3", result.CommandIdentity.Name);
            Assert.AreEqual("prefix3 sub3 name3", result.CommandIdentity.Prefix);
        }

        [TestMethod]
        public async Task PrefixMismatchMultipleWordsShouldError()
        {
            CommandExtractorContext context = new CommandExtractorContext("invalid_prefix3 sub3 name3 -key1=value1 -key2=value2");
            var result = await extractor.ExtractAsync(context);
            TestHelper.AssertOneImlxError(result, Errors.InvalidCommand, "The command string did not match any command prefix. command_string=invalid_prefix3 sub3 name3 -key1=value1 -key2=value2");
        }

        [TestMethod]
        public async Task PrefixMismatchShouldError()
        {
            CommandExtractorContext context = new CommandExtractorContext("invalidprefix -key1=value1 -key2=value2");
            var result = await extractor.ExtractAsync(context);
            TestHelper.AssertOneImlxError(result, Errors.InvalidCommand, "The command string did not match any command prefix. command_string=invalidprefix -key1=value1 -key2=value2");
        }

        [TestMethod]
        public async Task UnsupportedArgumentsShouldErrorMultipleAsync()
        {
            CommandExtractorContext context = new CommandExtractorContext("prefix1 -invalid_key1=value1 -invalid_key2=value2 -invalid_key3=value3");
            var result = await extractor.ExtractAsync(context);
            Assert.IsTrue(result.IsError);
            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(3, result.Errors.Length);
            TestHelper.AssertOneImlxError(result.Errors[0], Errors.UnsupportedArgument, "The command does not support the specified argument. command_name=name1 command_id=id1 argument=invalid_key1");
            TestHelper.AssertOneImlxError(result.Errors[1], Errors.UnsupportedArgument, "The command does not support the specified argument. command_name=name1 command_id=id1 argument=invalid_key2");
            TestHelper.AssertOneImlxError(result.Errors[2], Errors.UnsupportedArgument, "The command does not support the specified argument. command_name=name1 command_id=id1 argument=invalid_key3");
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
            Assert.IsFalse(result.IsError);

            Assert.IsNotNull(result.CommandIdentity);
            Assert.AreEqual("id2", result.CommandIdentity.Id);
            Assert.AreEqual("name2", result.CommandIdentity.Name);
            Assert.AreEqual("name2", result.CommandIdentity.Prefix);
            Assert.AreEqual("desc2", result.CommandIdentity.Description);
            Assert.AreEqual(typeof(CommandChecker), result.CommandIdentity.Checker);
            Assert.AreEqual(typeof(CommandRunner), result.CommandIdentity.Runner);
            Assert.IsNotNull(result.CommandIdentity.ArgumentIdentities);
            Assert.AreEqual(10, result.CommandIdentity.ArgumentIdentities.Count);
            AssertArgument(result.CommandIdentity.ArgumentIdentities[0], "key1", DataType.Text, false, "Key1 value text");
            AssertArgument(result.CommandIdentity.ArgumentIdentities[1], "key2", DataType.Text, true, "Key2 value text");
            AssertArgument(result.CommandIdentity.ArgumentIdentities[2], "key3", DataType.PhoneNumber, false, "Key3 value phone");
            AssertArgument(result.CommandIdentity.ArgumentIdentities[3], "key4", DataType.EmailAddress, false, "Key4 value email");
            AssertArgument(result.CommandIdentity.ArgumentIdentities[4], "key5", DataType.Url, false, "Key5 value url");
            AssertArgument(result.CommandIdentity.ArgumentIdentities[5], "key6", nameof(Boolean), false, "Key6 no value");
            AssertArgument(result.CommandIdentity.ArgumentIdentities[6], "key7", DataType.Currency, true, "Key7 value currency", new[] { "INR", "USD", "EUR" });
            AssertArgument(result.CommandIdentity.ArgumentIdentities[7], "key8", nameof(Int32), false, "Key8 value custom int");
            AssertArgument(result.CommandIdentity.ArgumentIdentities[8], "key9", nameof(Double), true, "Key9 value custom double", new object[] { 2.36, 25.36, 3669566.36, 26.36, -36985.25, 0, -5 });
            AssertArgument(result.CommandIdentity.ArgumentIdentities[9], "key10", nameof(String), true, "Key10 value custom string");

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
            Assert.IsFalse(result.IsError);

            // Args Supported
            Assert.IsNotNull(result.CommandIdentity);
            Assert.IsNotNull(result.CommandIdentity.ArgumentIdentities);
            Assert.AreEqual(10, result.CommandIdentity.ArgumentIdentities.Count);

            // No Args Passed
            Assert.IsNotNull(result.Command);
            Assert.IsNull(result.Command.Arguments);
        }

        [TestMethod]
        public async Task ValidCommandStringWithNoArgumentsShouldNotErrorAsync()
        {
            CommandExtractorContext context = new CommandExtractorContext("prefix4_noargs");
            var result = await extractor.ExtractAsync(context);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Command);
            Assert.IsNull(result.Command.Arguments);
        }

        [TestMethod]
        public async Task ValidCommandStringWithNoSeparatorShouldFailAsync()
        {
            CommandExtractorContext context = new CommandExtractorContext("prefix1-key1=value1-key2=value2");
            var result = await extractor.ExtractAsync(context);
            TestHelper.AssertOneImlxError(result, Errors.InvalidCommand, "The command separator is missing. command_string=prefix1-key1=value1-key2=value2");
        }

        protected override void OnTestInitialize()
        {
            options = MockCliOptions.New();
            commands = new InMemoryCommandIdentityStore(MockCommands.Commands, options, TestLogger.Create<InMemoryCommandIdentityStore>());
            argExtractor = new SeparatorArgumentExtractor(options, TestLogger.Create<SeparatorArgumentExtractor>());
            extractor = new SeparatorCommandExtractor(commands, argExtractor, options, TestLogger.Create<SeparatorCommandExtractor>());
        }

        private void AssertArgument(ArgumentIdentity arg, string name, DataType dataType, bool required = false, string? description = null, object[]? supportedValues = null)
        {
            Assert.AreEqual(arg.Name, name);
            Assert.AreEqual(arg.DataType, dataType);
            Assert.IsNull(arg.CustomDataType);
            Assert.AreEqual(arg.Required, required);
            Assert.AreEqual(arg.Description, description);
            CollectionAssert.AreEqual(arg.SupportedValues, supportedValues);
        }

        private void AssertArgument(ArgumentIdentity arg, string name, string customDataType, bool required = false, string? description = null, object[]? supportedValues = null)
        {
            Assert.AreEqual(arg.Name, name);
            Assert.AreEqual(arg.DataType, DataType.Custom);
            Assert.AreEqual(arg.CustomDataType, customDataType);
            Assert.AreEqual(arg.Required, required);
            Assert.AreEqual(arg.Description, description);
            CollectionAssert.AreEqual(arg.SupportedValues, supportedValues);
        }

        private void AssertArgument(Argument arg, string name, string customDataType, string description, object value)
        {
            Assert.AreEqual(arg.Name, name);
            Assert.AreEqual(arg.DataType, DataType.Custom);
            Assert.AreEqual(arg.CustomDataType, customDataType);
            Assert.AreEqual(arg.Description, description);
            Assert.AreEqual(arg.Value, value);
        }

        private void AssertArgument(Argument arg, string name, DataType dataType, string description, object value)
        {
            Assert.AreEqual(arg.Name, name);
            Assert.AreEqual(arg.DataType, dataType);
            Assert.IsNull(arg.CustomDataType);
            Assert.AreEqual(arg.Description, description);
            Assert.AreEqual(arg.Value, value);
        }

        private SeparatorArgumentExtractor argExtractor = null!;
        private ICommandIdentityStore commands = null!;
        private SeparatorCommandExtractor extractor = null!;
        private CliOptions options = null!;
    }
}
