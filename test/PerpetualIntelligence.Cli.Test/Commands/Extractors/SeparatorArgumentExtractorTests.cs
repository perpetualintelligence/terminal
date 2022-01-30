/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
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
    public class SeparatorArgumentExtractorTests : LogTest
    {
        public SeparatorArgumentExtractorTests() : base(TestLogger.Create<SeparatorArgumentExtractorTests>())
        {
        }

        [TestMethod]
        public async Task AttributeValueWithArgumentSeparatorShouldNotErrorAsync()
        {
            ArgumentExtractorContext context = new($"-key1=value=value2", command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"key1", result.Argument.Id);
            Assert.AreEqual($"value=value2", result.Argument.Value);
        }

        [TestMethod]
        public async Task AttributeValueWithCommandSeparatorSeparatorShouldNotErrorAsync()
        {
            ArgumentExtractorContext context = new($"-key1=value value2", command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"key1", result.Argument.Id);
            Assert.AreEqual($"value value2", result.Argument.Value);
        }

        [TestMethod]
        public async Task AttrubuteExtractShouldExtractNonKeyValueAsBooleanAsync()
        {
            ArgumentExtractorContext context = new($"-key6", command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual("key6", result.Argument.Id);
            Assert.AreEqual(System.ComponentModel.DataAnnotations.DataType.Custom, result.Argument.DataType);
            Assert.AreEqual(typeof(bool).Name, result.Argument.CustomDataType);
            Assert.IsTrue(Convert.ToBoolean(result.Argument.Value));
        }

        [TestMethod]
        public async Task CommandSeparatorAsArgumentPrefixShouldErrorAsync()
        {
            // space is the command separator
            ArgumentExtractorContext context = new($" key=value", command.Item1);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.InvalidArgument, $"The argument string does not have a valid prefix. command_name=name1 command_id=id1 arg= key=value prefix=-");
        }

        [TestMethod]
        public async Task CommandSupportArgsButUserPassedUnsupportedArgShouldErrorAsync()
        {
            ArgumentExtractorContext context = new ArgumentExtractorContext($"-not_supportedkey=value", command.Item1);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedArgument, "The argument is not supported. command_name=name1 command_id=id1 argument=not_supportedkey");
        }

        [TestMethod]
        public async Task EmptyAttributeNameAfterExtractShouldErrorAsync()
        {
            ArgumentExtractorContext context = new ArgumentExtractorContext($"-=value", command.Item1);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.InvalidArgument, "The argument id is null or empty. command_name=name1 command_id=id1 arg=-=value");
        }

        [TestMethod]
        public async Task EmptyAttributeNameAfterExtractWithNoPrefixShouldErrorAsync()
        {
            options.Extractor.ArgumentPrefix = null;
            ArgumentExtractorContext context = new($"=value", command.Item1);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.InvalidArgument, "The argument id is null or empty. command_name=name1 command_id=id1 arg==value");
        }

        [DataTestMethod]
        [DataRow("~", "`")]
        [DataRow("#", "öö")]
        [DataRow("öö", "मा")]
        [DataRow("öö", "-")]
        [DataRow("माणूस", "#")]
        public async Task InvalidArgumentValueSepratorShouldErrorAsync(string valid, string invalid)
        {
            // Set the correct separator
            options.Extractor.ArgumentSeparator = valid;

            // Arg string has incorrect separator Without the valid value separator the extractor will interpret as a
            // key only argument and that wil fail
            ArgumentExtractorContext context = new($"-key1{invalid}value1", command.Item1);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedArgument, $"The argument is not supported. command_name=name1 command_id=id1 argument=key1{invalid}value1");
        }

        [TestMethod]
        public async Task KeyAttrubuteExtractShouldSetTheArgumentNameAsync()
        {
            ArgumentExtractorContext context = new($"-key6", command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual("key6", result.Argument.Id);
        }

        [TestMethod]
        public async Task KeyValueAttrubuteExtractDoesNotSetTheArgumentNameAsync()
        {
            ArgumentExtractorContext context = new ArgumentExtractorContext($"-key5=htts://google.com", command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual("key5", result.Argument.Id);
        }

        [TestMethod]
        public async Task NullCommandIdentityShouldErrorAsync()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            ArgumentExtractorContext context = new("test_arg_string", null);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.InvalidRequest, "The command descriptor is missing in the request. extractor=PerpetualIntelligence.Cli.Commands.Extractors.SeparatorArgumentExtractor");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [TestMethod]
        public async Task NullOrWhiteSpaceArgumentStringShouldErrorAsync()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            ArgumentExtractorContext context = new ArgumentExtractorContext(null, command.Item1);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.InvalidArgument, "The argument string is missing in the request. command_name=name1 command_id=id1 extractor=PerpetualIntelligence.Cli.Commands.Extractors.SeparatorArgumentExtractor");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            context = new ArgumentExtractorContext("   ", command.Item1);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.InvalidArgument, "The argument string is missing in the request. command_name=name1 command_id=id1 extractor=PerpetualIntelligence.Cli.Commands.Extractors.SeparatorArgumentExtractor");
        }

        [TestMethod]
        public async Task PrefixDisabledButWithNoPrefixShouldNotErrorAsync()
        {
            options.Extractor.ArgumentPrefix = null;

            ArgumentExtractorContext context = new ArgumentExtractorContext($"key1=value1", command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"key1", result.Argument.Id);
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
        public async Task PrefixDisabledButWithPrefixShouldNotErrorAsync(string prefix)
        {
            options.Extractor.ArgumentPrefix = null;

            CommandDescriptor cmd = new CommandDescriptor("i1", "n1", "p1", new ArgumentDescriptors() { new ArgumentDescriptor($"{prefix}key", System.ComponentModel.DataAnnotations.DataType.Text) });
            ArgumentExtractorContext context = new ArgumentExtractorContext($"{prefix}key=value", cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"{prefix}key", result.Argument.Id);
            Assert.AreEqual($"value", result.Argument.Value);
        }

        [DataTestMethod]
        [DataRow("~", "`")]
        [DataRow("#", "öö")]
        [DataRow("-", "मा")]
        [DataRow("öö", "-")]
        [DataRow("माणूस", "#")]
        public async Task PrefixEnabledButInvalidPrefixShouldErrorAsync(string prefix, string invalidPrefix)
        {
            options.Extractor.ArgumentPrefix = prefix;

            ArgumentExtractorContext context = new ArgumentExtractorContext($"{invalidPrefix}key=value", command.Item1);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.InvalidArgument, $"The argument string does not have a valid prefix. command_name=name1 command_id=id1 arg={invalidPrefix}key=value prefix={prefix}");
        }

        [DataTestMethod]
        [DataRow("+")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("-")]
        [DataRow("öö")]
        [DataRow("माणूस")]
        public async Task PrefixEnabledButNotPrefixShouldErrorAsync(string prefix)
        {
            options.Extractor.ArgumentPrefix = prefix;

            ArgumentExtractorContext context = new ArgumentExtractorContext("key=value", command.Item1);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.InvalidArgument, $"The argument string does not have a valid prefix. command_name=name1 command_id=id1 arg=key=value prefix={prefix}");
        }

        [TestMethod]
        public async Task PrefixEnabledWithUnicodePrefixShouldNotErrorAsync()
        {
            options.Extractor.ArgumentPrefix = "माणू";

            CommandDescriptor cmd = new CommandDescriptor("i1", "n1", "p1", new ArgumentDescriptors() { new ArgumentDescriptor($"समाणूसkey1", System.ComponentModel.DataAnnotations.DataType.Text) });
            ArgumentExtractorContext context = new ArgumentExtractorContext($"माणूमाणूमाणूमाणूसमाणूसkey1=value1", cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"समाणूसkey1", result.Argument.Id);
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
        public async Task PrefixEnabledWithValidPrefixKeyOnlyShouldNotErrorAsync(string prefix)
        {
            options.Extractor.ArgumentPrefix = prefix;

            CommandDescriptor cmd = new("i1", "n1", "p1", new ArgumentDescriptors() { new ArgumentDescriptor("key", nameof(Boolean)) });
            ArgumentExtractorContext context = new ArgumentExtractorContext($"{prefix}key", cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual("key", result.Argument.Id);
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
        public async Task PrefixEnabledWithValidPrefixKeyValueShouldNotErrorAsync(string prefix)
        {
            options.Extractor.ArgumentPrefix = prefix;

            DateTime dateTime = DateTime.UtcNow;
            string value = $"{dateTime.ToLongDateString()}_{dateTime.ToLongTimeString()}";
            CommandDescriptor cmd = new("i1", "n1", "p1", new ArgumentDescriptors() { new ArgumentDescriptor("key", System.ComponentModel.DataAnnotations.DataType.DateTime) });
            ArgumentExtractorContext context = new ArgumentExtractorContext($"{prefix}key={value}", cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual("key", result.Argument.Id);
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
        public async Task SamePrefixAndSeparatorShouldNotErrorAsync(string input)
        {
            options.Extractor.ArgumentPrefix = input;
            options.Extractor.ArgumentSeparator = input;

            ArgumentExtractorContext context = new ArgumentExtractorContext($"{input}key1{input}value1", command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"key1", result.Argument.Id);
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
        public async Task ValidPrefixAndSepratorShouldNotErrorAsync(string prefix, string separator)
        {
            options.Extractor.ArgumentPrefix = prefix;
            options.Extractor.ArgumentSeparator = separator;

            ArgumentExtractorContext context = new ArgumentExtractorContext($"{prefix}key1{separator}value1", command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"key1", result.Argument.Id);
            Assert.AreEqual($"value1", result.Argument.Value);
        }

        protected override void OnTestInitialize()
        {
            command = MockCommands.NewCommandDefinition("id1", "name1", "prefix1", MockCommands.TestArgumentDescriptors, "desc1", null, null);
            options = MockCliOptions.New();
            extractor = new SeparatorArgumentExtractor(options, TestLogger.Create<SeparatorArgumentExtractor>());
        }

        private Tuple<CommandDescriptor, Command> command = null!;
        private SeparatorArgumentExtractor extractor = null!;
        private CliOptions options = null!;
    }
}
