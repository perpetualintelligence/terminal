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

        [DataTestMethod]
        [DataRow("~")]
        [DataRow(":")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("-")]
        [DataRow("öö")]
        [DataRow("मासे")]
        public async Task ArgumentAliasNotConfiguredButAliasPrefixUsedShouldErrorAsync(string aliasPrefix)
        {
            options.Extractor.ArgumentPrefix = "--";
            options.Extractor.ArgumentAliasPrefix = aliasPrefix;

            CommandDescriptor cmd = new("i1", "n1", "p1", new ArgumentDescriptors() { new ArgumentDescriptor("key1", System.ComponentModel.DataAnnotations.DataType.Text) });
            ArgumentExtractorContext context = new($"{aliasPrefix}key1=value1", isAlias: true, cmd);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.InvalidConfiguration, $"The argument extraction by alias prefix is not configured. argument_string={aliasPrefix}key1=value1");
        }

        [DataTestMethod]
        [DataRow("ü")]
        [DataRow(":")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("-")]
        [DataRow("--")]
        [DataRow("öö")]
        [DataRow("मासे")]
        [DataRow("女性")]
        public async Task ArgumentValueWithArgumentSeparatorShouldNotErrorAsync(string separator)
        {
            options.Extractor.ArgumentSeparator = separator;

            ArgumentExtractorContext context = new($"-key1{separator}value{separator}value2{separator}", command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"key1", result.Argument.Id);
            Assert.AreEqual($"value{separator}value2{separator}", result.Argument.Value);
        }

        [TestMethod]
        public async Task ArgumentWithoutPrefixShouldError()
        {
            // Argument extractor does not work with prefix
            CommandDescriptor cmd = new("i1", "n1", "p1", new ArgumentDescriptors() { new ArgumentDescriptor("key1", System.ComponentModel.DataAnnotations.DataType.Text) });
            ArgumentExtractorContext context = new($"key1=value1", cmd);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.InvalidArgument, $"The argument string is not valid. argument_string=key1=value1");
        }

        [TestMethod]
        public async Task ArgumentWithoutPrefixShouldErrorAsync()
        {
            ArgumentExtractorContext context = new ArgumentExtractorContext($"key1=value", command.Item1);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.InvalidArgument, "The argument string is not valid. argument_string=key1=value");
        }

        [TestMethod]
        public async Task AttrubuteShouldSetTheArgumentIdCorrectlyAsync()
        {
            ArgumentExtractorContext context = new("-key6", command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual("key6", result.Argument.Id);
        }

        [TestMethod]
        public async Task EmptyArgumentIdShouldErrorAsync()
        {
            ArgumentExtractorContext context = new($"-  =value", command.Item1);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.InvalidArgument, "The argument identifier is null or empty. argument_string=-  =value");
        }

        [DataTestMethod]
        [DataRow("~", "`")]
        [DataRow("#", "öö")]
        [DataRow("öö", "मा")]
        [DataRow("öö", "-")]
        [DataRow("मासे", "#")]
        public async Task InvalidArgumentValueSepratorShouldErrorAsync(string valid, string invalid)
        {
            // Set the correct separator
            options.Extractor.ArgumentSeparator = valid;

            // Arg string has incorrect separator Without the valid value separator the extractor will interpret as a
            // key only argument and that wil fail
            ArgumentExtractorContext context = new($"-key1{invalid}value1", command.Item1);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedArgument, $"The argument is not supported. argument=key1{invalid}value1");
        }

        [DataTestMethod]
        [DataRow("=")]
        [DataRow(" ")]
        [DataRow(":")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("-")]
        [DataRow("--")]
        [DataRow("öö")]
        [DataRow("मासे")]
        [DataRow("女性")]
        [DataRow("माणू女性")]
        public async Task KeySeparatorValueShouldNotErrorAsync(string argSeparator)
        {
            options.Extractor.ArgumentSeparator = argSeparator;

            ArgumentExtractorContext context = new($"-key1{argSeparator}value1", command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"key1", result.Argument.Id);
            Assert.AreEqual($"value1", result.Argument.Value);
        }

        [TestMethod]
        public async Task KeyValueArgumentShouldSetTheArgumentIdAndValueCorrecltyAsync()
        {
            ArgumentExtractorContext context = new ArgumentExtractorContext($"-key5=htts://google.com", command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual("key5", result.Argument.Id);
            Assert.AreEqual("htts://google.com", result.Argument.Value);
        }

        [DataTestMethod]
        [DataRow("ü")]
        [DataRow(":")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("-")]
        [DataRow("--")]
        [DataRow("öö")]
        [DataRow("मासे")]
        [DataRow("女性")]
        public async Task MultiplePrefixBeforeArgumentIdShouldNotErrorAsync(string prefix)
        {
            options.Extractor.ArgumentPrefix = prefix;

            CommandDescriptor cmd = new("i1", "n1", "p1", new ArgumentDescriptors() { new ArgumentDescriptor($"key1", System.ComponentModel.DataAnnotations.DataType.Text) });
            ArgumentExtractorContext context = new($"{prefix}{prefix}{prefix}key1=value1", cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"key1", result.Argument.Id);
            Assert.AreEqual($"value1", result.Argument.Value);
        }

        [DataTestMethod]
        [DataRow("ü")]
        [DataRow(":")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("-")]
        [DataRow("öö")]
        [DataRow("मासे")]
        [DataRow("女性")]
        public async Task MultiplePrefixNotMatchingArgumentIdShouldErrorAsync(string prefix)
        {
            options.Extractor.ArgumentPrefix = prefix;

            CommandDescriptor cmd = new("i1", "n1", "p1", new ArgumentDescriptors() { new ArgumentDescriptor($"key1", System.ComponentModel.DataAnnotations.DataType.Text) });
            ArgumentExtractorContext context = new($"{prefix}{prefix}{prefix}{prefix}key=value1", cmd);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedArgument, $"The argument is not supported. argument=key");
        }

        [TestMethod]
        public void NullCommandDescriptorShouldError()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CA1806 // Do not ignore method results
            TestHelper.AssertThrowsWithMessage<ArgumentException>(() => new ArgumentExtractorContext("test_arg_string", null), "Value cannot be null. (Parameter 'commandDescriptor')");
#pragma warning restore CA1806 // Do not ignore method results
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [TestMethod]
        public void NullOrWhiteSpaceArgumentStringShouldError()
        {
#pragma warning disable CA1806 // Do not ignore method results
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            TestHelper.AssertThrowsWithMessage<ArgumentException>(() => new ArgumentExtractorContext(null, command.Item1), "'argumentString' cannot be null or whitespace. (Parameter 'argumentString')");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            TestHelper.AssertThrowsWithMessage<ArgumentException>(() => new ArgumentExtractorContext("   ", command.Item1), "'argumentString' cannot be null or whitespace. (Parameter 'argumentString')");
#pragma warning restore CA1806 // Do not ignore method results
        }

        [DataTestMethod]
        [DataRow(":")]
        [DataRow("+")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("-")]
        [DataRow("öö")]
        [DataRow("मासे")]
        [DataRow("女性")]
        public async Task PrefixEmptyButArgumentIdWithPrefixShouldNotErrorAsync(string prefix)
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            options.Extractor.ArgumentPrefix = "";
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            CommandDescriptor cmd = new CommandDescriptor("i1", "n1", "p1", new ArgumentDescriptors() { new ArgumentDescriptor($"{prefix}key", System.ComponentModel.DataAnnotations.DataType.Text) });
            ArgumentExtractorContext context = new ArgumentExtractorContext($"{prefix}key=value", cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"{prefix}key", result.Argument.Id);
            Assert.AreEqual($"value", result.Argument.Value);
        }

        [TestMethod]
        public async Task UnicodePrefixInValueShouldNotErrorAsync()
        {
            options.Extractor.ArgumentPrefix = "माणू";

            CommandDescriptor cmd = new CommandDescriptor("i1", "n1", "p1", new ArgumentDescriptors() { new ArgumentDescriptor("स", System.ComponentModel.DataAnnotations.DataType.Text) });
            ArgumentExtractorContext context = new($"माणूमाणूस=माणूसमास", cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"स", result.Argument.Id);
            Assert.AreEqual($"माणूसमास", result.Argument.Value);
        }

        [DataTestMethod]
        [DataRow("ü")]
        [DataRow(":")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("-")]
        [DataRow("--")]
        [DataRow("öö")]
        [DataRow("मासे")]
        [DataRow("女性")]
        public async Task PrefixInArgumentIdShouldErrorAsync(string prefix)
        {
            options.Extractor.ArgumentPrefix = prefix;

            // Prefix are not allowed in argument id
            CommandDescriptor cmd = new("i1", "n1", "p1", new ArgumentDescriptors() { new ArgumentDescriptor($"{prefix}key1", System.ComponentModel.DataAnnotations.DataType.Text) });
            ArgumentExtractorContext context = new($"{prefix}key1=value1", cmd);

            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedArgument, "The argument is not supported. argument=key1");
        }

        [DataTestMethod]
        [DataRow("ü")]
        [DataRow(":")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("-")]
        [DataRow("--")]
        [DataRow("öö")]
        [DataRow("मासे")]
        [DataRow("女性")]
        public async Task PrefixInArgumentValueShouldNotErrorAsync(string prefix)
        {
            options.Extractor.ArgumentPrefix = prefix;

            // Prefix are not allowed in argument id
            CommandDescriptor cmd = new("i1", "n1", "p1", new ArgumentDescriptors() { new ArgumentDescriptor($"key1", System.ComponentModel.DataAnnotations.DataType.Text) });
            ArgumentExtractorContext context = new($"{prefix}key1={prefix}value1{prefix}", cmd);
            var result = await extractor.ExtractAsync(context);

            Assert.AreEqual("key1", result.Argument.Id);
            Assert.AreEqual($"{prefix}value1{prefix}", result.Argument.Value);
        }

        [TestMethod]
        public async Task UnicodeArgStringDoesNotStartWithSeparatorShouldExtractCorrectly()
        {
            options.Extractor.Separator = "स";
            options.Extractor.ArgumentPrefix = "ई";
            options.Extractor.ArgumentSeparator = "र";

            CommandDescriptor cmd = new CommandDescriptor("i1", "n1", "p1", new ArgumentDescriptors() { new ArgumentDescriptor($"पक्षी", System.ComponentModel.DataAnnotations.DataType.Text) });
            ArgumentExtractorContext context = new($"ईपक्षीरप्राणीप्रेम", cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"पक्षी", result.Argument.Id);
            Assert.AreEqual($"प्राणीप्रेम", result.Argument.Value);
        }

        [TestMethod]
        public async Task UnicodeArgStringNoArgSeparatorForBooleanShouldNotError()
        {
            options.Extractor.Separator = "स";
            options.Extractor.ArgumentPrefix = "ई";
            options.Extractor.ArgumentSeparator = "र";

            CommandDescriptor cmd = new CommandDescriptor("i1", "n1", "p1", new ArgumentDescriptors() { new ArgumentDescriptor($"पक्षी", nameof(Boolean)) });
            ArgumentExtractorContext context = new($"ईपक्षी", cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"पक्षी", result.Argument.Id);
            Assert.AreEqual("True", result.Argument.Value);
        }

        [TestMethod]
        public async Task UnicodeArgStringNoArgSeparatorShouldErrorAsync()
        {
            options.Extractor.Separator = "स";
            options.Extractor.ArgumentPrefix = "ई";
            options.Extractor.ArgumentSeparator = "र";

            CommandDescriptor cmd = new CommandDescriptor("i1", "n1", "p1", new ArgumentDescriptors() { new ArgumentDescriptor($"पक्षी", System.ComponentModel.DataAnnotations.DataType.Text) });
            ArgumentExtractorContext context = new($"ईपक्षी", cmd);

            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.InvalidArgument, "The argument value is missing. argument_string=ईपक्षीर");
        }

        [TestMethod]
        public async Task UnicodeArgStringNoValueShouldNotErrorAsync()
        {
            options.Extractor.Separator = "स";
            options.Extractor.ArgumentPrefix = "ई";
            options.Extractor.ArgumentSeparator = "र";

            CommandDescriptor cmd = new CommandDescriptor("i1", "n1", "p1", new ArgumentDescriptors() { new ArgumentDescriptor($"पक्षी", System.ComponentModel.DataAnnotations.DataType.Text) });
            ArgumentExtractorContext context = new($"ईपक्षीर", cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"पक्षी", result.Argument.Id);
            Assert.AreEqual("", result.Argument.Value);
        }

        [TestMethod]
        public async Task UnicodeArgStringStartsWithArgSeparatorShouldErrorAsync()
        {
            options.Extractor.Separator = "स";
            options.Extractor.ArgumentPrefix = "ई";
            options.Extractor.ArgumentSeparator = "र";

            CommandDescriptor cmd = new CommandDescriptor("i1", "n1", "p1", new ArgumentDescriptors() { new ArgumentDescriptor($"पक्षी", System.ComponentModel.DataAnnotations.DataType.Text) });
            ArgumentExtractorContext context = new($"रईपक्षीप्राणीप्रेम", cmd);

            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.InvalidArgument, "The argument string is not valid. argument_string=रईपक्षीप्राणीप्रेम");
        }

        [TestMethod]
        public async Task UnicodeArgStringStartsWithSeparatorShouldExtractCorrectly()
        {
            options.Extractor.Separator = "स";
            options.Extractor.ArgumentPrefix = "ई";
            options.Extractor.ArgumentSeparator = "र";

            CommandDescriptor cmd = new CommandDescriptor("i1", "n1", "p1", new ArgumentDescriptors() { new ArgumentDescriptor($"पक्षी", System.ComponentModel.DataAnnotations.DataType.Text) });
            ArgumentExtractorContext context = new($"सईपक्षीरप्राणीप्रेम", cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"पक्षी", result.Argument.Id);
            Assert.AreEqual($"प्राणीप्रेम", result.Argument.Value);
        }

        [TestMethod]
        public async Task UnicodeArgStringValueNotWithinShouldNotErrorAsync()
        {
            options.Extractor.Separator = "स";
            options.Extractor.ArgumentPrefix = "ई";
            options.Extractor.ArgumentSeparator = "र";
            options.Extractor.ArgumentValueWithIn = "बी";

            CommandDescriptor cmd = new CommandDescriptor("i1", "n1", "p1", new ArgumentDescriptors() { new ArgumentDescriptor($"पक्षी", System.ComponentModel.DataAnnotations.DataType.Text) });
            ArgumentExtractorContext context = new($"ईपक्षीरप्राणीप्रेमबी", cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"पक्षी", result.Argument.Id);
            Assert.AreEqual($"प्राणीप्रेमबी", result.Argument.Value);
        }

        [TestMethod]
        public async Task UnicodeArgStringValueWithinShouldNotErrorAsync()
        {
            options.Extractor.Separator = "स";
            options.Extractor.ArgumentPrefix = "ई";
            options.Extractor.ArgumentSeparator = "र";
            options.Extractor.ArgumentValueWithIn = "बी";

            CommandDescriptor cmd = new CommandDescriptor("i1", "n1", "p1", new ArgumentDescriptors() { new ArgumentDescriptor($"पक्षी", System.ComponentModel.DataAnnotations.DataType.Text) });
            ArgumentExtractorContext context = new($"ईपक्षीरबीप्राणीप्रेमबी", cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"पक्षी", result.Argument.Id);
            Assert.AreEqual($"प्राणीप्रेम", result.Argument.Value);
        }

        [TestMethod]
        public async Task UnicodeArgStringWithMultiplePrefixAndSeparatorShouldNotErrorAsync()
        {
            options.Extractor.Separator = "स";
            options.Extractor.ArgumentPrefix = "ई";
            options.Extractor.ArgumentSeparator = "र";

            CommandDescriptor cmd = new CommandDescriptor("i1", "n1", "p1", new ArgumentDescriptors() { new ArgumentDescriptor($"पक्षी", System.ComponentModel.DataAnnotations.DataType.Text) });
            ArgumentExtractorContext context = new($"ससससससईईईईईईईईपक्षीरररररररररप्राणीप्रेमसससससस", cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"पक्षी", result.Argument.Id);
            Assert.AreEqual($"प्राणीप्रेम", result.Argument.Value);
        }

        [TestMethod]
        public async Task UsupportedArgumentShouldErrorAsync()
        {
            ArgumentExtractorContext context = new ArgumentExtractorContext($"-invalid=value", command.Item1);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedArgument, "The argument is not supported. argument=invalid");
        }

        [DataTestMethod]
        [DataRow("key1_alias")]
        [DataRow("öö")]
        [DataRow("मासे")]
        [DataRow("女性")]
        public async Task ValidArgAliasOnlyShouldNotErrorAsync(string keyAlias)
        {
            options.Extractor.ArgumentAlias = true;
            options.Extractor.ArgumentPrefix = "-";
            options.Extractor.ArgumentAliasPrefix = "--";

            CommandDescriptor cmd = new("i1", "n1", "p1", new ArgumentDescriptors() { new ArgumentDescriptor("key1", nameof(Boolean)) { Alias = keyAlias } });
            ArgumentExtractorContext context = new ArgumentExtractorContext($"--{keyAlias}", isAlias:true, cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual("key1", result.Argument.Id);
            Assert.AreEqual(System.ComponentModel.DataAnnotations.DataType.Custom, result.Argument.DataType);
            Assert.AreEqual(nameof(Boolean), result.Argument.CustomDataType);
            Assert.AreEqual("True", result.Argument.Value);
        }

        [DataTestMethod]
        [DataRow("key1")]
        [DataRow("öö")]
        [DataRow("मासे")]
        [DataRow("女性")]
        public async Task ValidArgIdOnlyShouldNotErrorAsync(string key)
        {
            CommandDescriptor cmd = new("i1", "n1", "p1", new ArgumentDescriptors() { new ArgumentDescriptor(key, nameof(Boolean)) });
            ArgumentExtractorContext context = new ArgumentExtractorContext($"-{key}", cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual(key, result.Argument.Id);
            Assert.AreEqual(System.ComponentModel.DataAnnotations.DataType.Custom, result.Argument.DataType);
            Assert.AreEqual(nameof(Boolean), result.Argument.CustomDataType);
            Assert.AreEqual("True", result.Argument.Value);
        }

        [TestMethod]
        [DataTestMethod]
        [DataRow("\"")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("मासे")]
        [DataRow("女性")]
        public async Task WithInConfiguredButNotUsedShouldExtractCorrectlyAsync(string withIn)
        {
            options.Extractor.ArgumentValueWithIn = withIn;

            ArgumentExtractorContext context = new($"-key1=test string with {withIn} in between and end but not at start {withIn}", command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"key1", result.Argument.Id);
            Assert.AreEqual($"test string with {withIn} in between and end but not at start {withIn}", result.Argument.Value);
        }

        [TestMethod]
        [DataTestMethod]
        [DataRow("\"")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("मासे")]
        [DataRow("女性")]
        public async Task WithInConfiguredShouldExtractCorrectlyAsync(string withIn)
        {
            options.Extractor.ArgumentValueWithIn = withIn;

            ArgumentExtractorContext context = new($"-key1={withIn}test string with {withIn} in between {withIn}", command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"key1", result.Argument.Id);
            Assert.AreEqual($"test string with {withIn} in between ", result.Argument.Value);
        }

        [TestMethod]
        [DataTestMethod]
        [DataRow("\"")]
        [DataRow("~")]
        [DataRow("#")]
        [DataRow("sp")]
        [DataRow("öö")]
        [DataRow("मासे")]
        [DataRow("女性")]
        public async Task WithInNotConfiguredShouldExtractCorrectlyAsync(string withIn)
        {
            options.Extractor.ArgumentValueWithIn = null;

            ArgumentExtractorContext context = new($"-key1={withIn}test string with {withIn} in between {withIn}", command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Argument);
            Assert.AreEqual($"key1", result.Argument.Id);
            Assert.AreEqual($"{withIn}test string with {withIn} in between {withIn}", result.Argument.Value);
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
