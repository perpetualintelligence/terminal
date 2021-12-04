/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    [TestClass]
    public class SeparatorArgumentExtractorTests : OneImlxLogTest
    {
        public SeparatorArgumentExtractorTests() : base(TestLogger.Create<SeparatorArgumentExtractorTests>())
        {
        }

        [TestMethod]
        public async Task AttrubuteExtractShouldExtractNonKeyValueAsBooleanAsync()
        {
            ArgumentExtractorContext context = new ArgumentExtractorContext($"-key6", command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual("key6", result.Argument.Name);
            Assert.AreEqual(System.ComponentModel.DataAnnotations.DataType.Custom, result.Argument.DataType);
            Assert.AreEqual(typeof(bool).Name, result.Argument.CustomDataType);
            Assert.IsTrue(Convert.ToBoolean(result.Argument.Value));
        }

        [TestMethod]
        public async Task EmptyAttributeNameAfterExtractShouldErrorAsync()
        {
            ArgumentExtractorContext context = new ArgumentExtractorContext($"-=value", command.Item1);
            var result = await extractor.ExtractAsync(context);
            TestHelper.AssertOneImlxError(result, Errors.InvalidArgument, "The argument name is null or empty. command_name=name1 command_id=id1 arg=-=value");
        }

        [TestMethod]
        public async Task EmptyAttributeNameAfterExtractWithNoPrefixShouldErrorAsync()
        {
            options.Extractor.ArgumentPrefix = null;
            ArgumentExtractorContext context = new ArgumentExtractorContext($"=value", command.Item1);
            var result = await extractor.ExtractAsync(context);
            TestHelper.AssertOneImlxError(result, Errors.InvalidArgument, "The argument name is null or empty. command_name=name1 command_id=id1 arg==value");
        }

        [DataTestMethod]
        [DataRow("~", "`")]
        [DataRow("#", "öö")]
        [DataRow("öö", "मा")]
        [DataRow("öö", "-")]
        [DataRow("माणूस", "#")]
        public async Task InvalidArgumentValueSepratorShouldFailAsync(string valid, string invalid)
        {
            // Set the correct separator
            options.Extractor.ArgumentValueSeparator = valid;

            // Arg string has incorrect separator
            // Without the valid value separator the extractor will interpret as a key only argument and that wil fail
            ArgumentExtractorContext context = new ArgumentExtractorContext($"-key1{invalid}value1", command.Item1);
            var result = await extractor.ExtractAsync(context);
            TestHelper.AssertOneImlxError(result, Errors.UnsupportedArgument, $"The command does not support the request argument. command_name=name1 command_id=id1 argument_name=key1{invalid}value1");
        }

        [TestMethod]
        public async Task InvalidAttributeSyntaxShouldErrorAsync()
        {
            ArgumentExtractorContext context = new ArgumentExtractorContext($"-key=value=value2", command.Item1);
            var result = await extractor.ExtractAsync(context);
            TestHelper.AssertOneImlxError(result, Errors.InvalidArgument, "The argument syntax is not valid. command_name=name1 command_id=id1 argument_string=-key=value=value2");
        }

        [TestMethod]
        public async Task KeyAttrubuteExtractShouldSetTheArgumentIdAsync()
        {
            ArgumentExtractorContext context = new ArgumentExtractorContext($"-key6", command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual("aid6", result.Argument.Id);
        }

        [TestMethod]
        public async Task KeyValueAttrubuteExtractDoesNotSetTheArgumentIdAsync()
        {
            ArgumentExtractorContext context = new ArgumentExtractorContext($"-key5=htts://google.com", command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual("aid5", result.Argument.Id);
        }

        [TestMethod]
        public async Task NullCommandIdentityShoudErrorAsync()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            ArgumentExtractorContext context = new("test_arg_string", null);
            var result = await extractor.ExtractAsync(context);
            TestHelper.AssertOneImlxError(result, Errors.InvalidRequest, "The command identity is missing in the request. extractor=PerpetualIntelligence.Cli.Commands.Extractors.SeparatorArgumentExtractor");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [TestMethod]
        public async Task NullOrWhiteSpaceArgumentStringShoudErrorAsync()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            ArgumentExtractorContext context = new ArgumentExtractorContext(null, command.Item1);
            var result = await extractor.ExtractAsync(context);
            TestHelper.AssertOneImlxError(result, Errors.InvalidArgument, "The argument string is missing in the request. command_name=name1 command_id=id1 extractor=PerpetualIntelligence.Cli.Commands.Extractors.SeparatorArgumentExtractor");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            context = new ArgumentExtractorContext("   ", command.Item1);
            result = await extractor.ExtractAsync(context);
            TestHelper.AssertOneImlxError(result, Errors.InvalidArgument, "The argument string is missing in the request. command_name=name1 command_id=id1 extractor=PerpetualIntelligence.Cli.Commands.Extractors.SeparatorArgumentExtractor");
        }

        [TestMethod]
        public async Task PrefixDisabledButWithNoPrefixShoudNotErrorAsync()
        {
            options.Extractor.ArgumentPrefix = null;

            ArgumentExtractorContext context = new ArgumentExtractorContext($"key1=value1", command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"key1", result.Argument.Name);
            Assert.AreEqual($"value1", result.Argument.Value);
        }

        [DataTestMethod]
        [DataRow("+")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("-")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task PrefixDisabledButWithPrefixShoudNotErrorAsync(string prefix)
        {
            options.Extractor.ArgumentPrefix = null;

            CommandIdentity cmd = new CommandIdentity("i1", "n1", "p1", new ArgumentIdentities() { new ArgumentIdentity("ai1", $"{prefix}key", System.ComponentModel.DataAnnotations.DataType.Text) });
            ArgumentExtractorContext context = new ArgumentExtractorContext($"{prefix}key=value", cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"{prefix}key", result.Argument.Name);
            Assert.AreEqual($"value", result.Argument.Value);
        }

        [DataTestMethod]
        [DataRow("~", "`")]
        [DataRow("#", "öö")]
        [DataRow("-", "मा")]
        [DataRow("öö", "-")]
        [DataRow("माणूस", "#")]
        public async Task PrefixEnabledButInvalidPrefixShoudErrorAsync(string prefix, string invalidPrefix)
        {
            options.Extractor.ArgumentPrefix = prefix;

            ArgumentExtractorContext context = new ArgumentExtractorContext($"{invalidPrefix}key=value", command.Item1);
            var result = await extractor.ExtractAsync(context);
            TestHelper.AssertOneImlxError(result, Errors.InvalidArgument, $"The argument string does not have a valid prefix. command_name=name1 command_id=id1 arg={invalidPrefix}key=value prefix={prefix}");
        }

        [DataTestMethod]
        [DataRow("+")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("-")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        public async Task PrefixEnabledButNotPrefixShoudErrorAsync(string prefix)
        {
            options.Extractor.ArgumentPrefix = prefix;

            ArgumentExtractorContext context = new ArgumentExtractorContext("key=value", command.Item1);
            var result = await extractor.ExtractAsync(context);
            TestHelper.AssertOneImlxError(result, Errors.InvalidArgument, $"The argument string does not have a valid prefix. command_name=name1 command_id=id1 arg=key=value prefix={prefix}");
        }

        [TestMethod]
        public async Task PrefixEnabledWithUnicodePrefixShoudNotErrorAsync()
        {
            options.Extractor.ArgumentPrefix = "माणू";

            CommandIdentity cmd = new CommandIdentity("i1", "n1", "p1", new ArgumentIdentities() { new ArgumentIdentity("ai1", $"समाणूसkey1", System.ComponentModel.DataAnnotations.DataType.Text) });
            ArgumentExtractorContext context = new ArgumentExtractorContext($"माणूमाणूमाणूमाणूसमाणूसkey1=value1", cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"समाणूसkey1", result.Argument.Name);
            Assert.AreEqual($"value1", result.Argument.Value);
        }

        [DataTestMethod]
        [DataRow("+")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("-")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task PrefixEnabledWithValidPrefixKeyOnlyShoudNotErrorAsync(string prefix)
        {
            options.Extractor.ArgumentPrefix = prefix;

            CommandIdentity cmd = new("i1", "n1", "p1", new ArgumentIdentities() { new ArgumentIdentity("ai1", "key", nameof(Boolean)) });
            ArgumentExtractorContext context = new ArgumentExtractorContext($"{prefix}key", cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual("key", result.Argument.Name);
            Assert.AreEqual(System.ComponentModel.DataAnnotations.DataType.Custom, result.Argument.DataType);
            Assert.AreEqual(nameof(Boolean), result.Argument.CustomDataType);
            Assert.AreEqual(true, result.Argument.Value);
        }

        [DataTestMethod]
        [DataRow("+")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("-")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task PrefixEnabledWithValidPrefixKeyValueShoudNotErrorAsync(string prefix)
        {
            options.Extractor.ArgumentPrefix = prefix;

            DateTime dateTime = DateTime.UtcNow;
            string value = $"{dateTime.ToLongDateString()}_{dateTime.ToLongTimeString()}";
            CommandIdentity cmd = new("i1", "n1", "p1", new ArgumentIdentities() { new ArgumentIdentity("ai1", "key", System.ComponentModel.DataAnnotations.DataType.DateTime) });
            ArgumentExtractorContext context = new ArgumentExtractorContext($"{prefix}key={value}", cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual("key", result.Argument.Name);
            Assert.AreEqual(System.ComponentModel.DataAnnotations.DataType.DateTime, result.Argument.DataType);
            Assert.AreEqual(value, result.Argument.Value);
        }

        [DataTestMethod]
        [DataRow("+")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("-")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        [DataRow("女性")]
        public async Task SamePrefixAndSeparatorShoudNotErrorAsync(string input)
        {
            options.Extractor.ArgumentPrefix = input;
            options.Extractor.ArgumentValueSeparator = input;

            ArgumentExtractorContext context = new ArgumentExtractorContext($"{input}key1{input}value1", command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"key1", result.Argument.Name);
            Assert.AreEqual($"value1", result.Argument.Value);
        }

        [DataTestMethod]
        [DataRow("-", "=")]
        [DataRow("~", "#")]
        [DataRow("=", "-")]
        [DataRow("णूस", "मा")]
        [DataRow("माणूस", "öö")]
        [DataRow("माणूस", "~")]
        [DataRow("-", "女性")]
        public async Task ValidPrefixAndSepratorShoudNotErrorAsync(string prefix, string separator)
        {
            options.Extractor.ArgumentPrefix = prefix;
            options.Extractor.ArgumentValueSeparator = separator;

            ArgumentExtractorContext context = new ArgumentExtractorContext($"{prefix}key1{separator}value1", command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"key1", result.Argument.Name);
            Assert.AreEqual($"value1", result.Argument.Value);
        }

        protected override void OnTestInitialize()
        {
            command = MockCommands.NewCommand("id1", "name1", "prefix1", "desc1", null, null, MockCommands.ArgumentIdentities);
            options = MockCliOptions.New();
            extractor = new SeparatorArgumentExtractor(options, TestLogger.Create<SeparatorArgumentExtractor>());
        }

        private Tuple<CommandIdentity, Command> command = null!;
        private SeparatorArgumentExtractor extractor = null!;
        private CliOptions options = null!;
    }
}
