/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Mocks;

using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    [TestClass]
    public class OptionExtractorTests : InitializerTests
    {
        public OptionExtractorTests() : base(TestLogger.Create<OptionExtractorTests>())
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
        public async Task OptionAliasNotConfiguredButAliasPrefixUsedShouldErrorAsync(string aliasPrefix)
        {
            options.Extractor.OptionPrefix = "--";
            options.Extractor.OptionAliasPrefix = aliasPrefix;

            CommandDescriptor cmd = new("i1", "n1", "p1", "desc1", new OptionDescriptors(textHandler, new[] { new OptionDescriptor("key1", System.ComponentModel.DataAnnotations.DataType.Text, "desc1") }));
            OptionExtractorContext context = new(new OptionString($"{aliasPrefix}key1=value1", aliasPrefix: true, 0), cmd);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.InvalidConfiguration, $"The option extraction by alias prefix is not configured. option_string={aliasPrefix}key1=value1");
        }

        [TestMethod]
        public void ArgumentExtractor_RegexPatterns_ShouldBeValid()
        {
            Assert.AreEqual("^[ ]*(-)+(.+?)[ ]*$", extractor.OptionAliasNoValueRegexPattern);
            Assert.AreEqual("^[ ]*(-)+(.+?)=+(.*?)[ ]*$", extractor.OptionAliasValueRegexPattern);
            Assert.AreEqual("^[ ]*(-)+(.+?)[ ]*$", extractor.OptionIdNoValueRegexPattern);
            Assert.AreEqual("^[ ]*(-)+(.+?)=+(.*?)[ ]*$", extractor.OptionIdValueRegexPattern);
            Assert.AreEqual("^(.*)$", extractor.OptionValueWithinRegexPattern);
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
        public async Task OptionValueWithOptionSeparatorShouldNotErrorAsync(string separator)
        {
            options.Extractor.OptionValueSeparator = separator;

            OptionExtractorContext context = new(new OptionString($"-key1{separator}value{separator}value2{separator}"), command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Option);
            Assert.AreEqual($"key1", result.Option.Id);
            Assert.AreEqual($"value{separator}value2{separator}", result.Option.Value);
        }

        [TestMethod]
        public async Task ArgumentWithoutPrefixShouldError()
        {
            // Option extractor does not work with prefix
            CommandDescriptor cmd = new("i1", "n1", "p1", "desc1", new OptionDescriptors(textHandler, new[] { new OptionDescriptor("key1", System.ComponentModel.DataAnnotations.DataType.Text, "desc1") }));
            OptionExtractorContext context = new(new OptionString($"key1=value1"), cmd);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.InvalidOption, $"The option string is not valid. option_string=key1=value1");
        }

        [TestMethod]
        public async Task ArgumentWithoutPrefixShouldErrorAsync()
        {
            OptionExtractorContext context = new(new OptionString($"key1=value"), command.Item1);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.InvalidOption, "The option string is not valid. option_string=key1=value");
        }

        [TestMethod]
        public async Task AttrubuteShouldSetTheArgumentIdCorrectlyAsync()
        {
            OptionExtractorContext context = new(new OptionString("-key6"), command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Option);
            Assert.AreEqual("key6", result.Option.Id);
        }

        [TestMethod]
        public async Task EmptyArgumentIdShouldErrorAsync()
        {
            OptionExtractorContext context = new(new OptionString($"-  =value"), command.Item1);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.InvalidOption, "The option identifier is null or empty. option_string=-  =value");
        }

        [DataTestMethod]
        [DataRow("~", "`")]
        [DataRow("#", "öö")]
        [DataRow("öö", "मा")]
        [DataRow("öö", "-")]
        [DataRow("मासे", "#")]
        public async Task InvalidOptionValueSepratorShouldErrorAsync(string valid, string invalid)
        {
            // Set the correct separator
            options.Extractor.OptionValueSeparator = valid;

            // Arg string has incorrect separator Without the valid value separator the extractor will interpret as a
            // key only option and that wil fail
            OptionExtractorContext context = new(new OptionString($"-key1{invalid}value1"), command.Item1);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedOption, $"The option is not supported. option=key1{invalid}value1");
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
            options.Extractor.OptionValueSeparator = argSeparator;

            OptionExtractorContext context = new(new OptionString($"-key1{argSeparator}value1"), command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Option);
            Assert.AreEqual($"key1", result.Option.Id);
            Assert.AreEqual($"value1", result.Option.Value);
        }

        [TestMethod]
        public async Task KeyValueArgumentShouldSetTheArgumentIdAndValueCorrecltyAsync()
        {
            OptionExtractorContext context = new(new OptionString($"-key5=htts://google.com"), command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Option);
            Assert.AreEqual("key5", result.Option.Id);
            Assert.AreEqual("htts://google.com", result.Option.Value);
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
            options.Extractor.OptionPrefix = prefix;

            CommandDescriptor cmd = new("i1", "n1", "p1", "desc1", new OptionDescriptors(textHandler, new[] { new OptionDescriptor("key1", System.ComponentModel.DataAnnotations.DataType.Text, "desc1") }));
            OptionExtractorContext context = new(new OptionString($"{prefix}{prefix}{prefix}key1=value1"), cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Option);
            Assert.AreEqual($"key1", result.Option.Id);
            Assert.AreEqual($"value1", result.Option.Value);
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
            options.Extractor.OptionPrefix = prefix;

            CommandDescriptor cmd = new("i1", "n1", "p1", "desc1", new OptionDescriptors(textHandler, new[] { new OptionDescriptor("key1", System.ComponentModel.DataAnnotations.DataType.Text, "desc1") }));
            OptionExtractorContext context = new(new OptionString($"{prefix}{prefix}{prefix}{prefix}key=value1"), cmd);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedOption, $"The option is not supported. option=key");
        }

        [TestMethod]
        public void NullCommandDescriptorShouldError()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CA1806 // Do not ignore method results
            TestHelper.AssertThrowsWithMessage<ArgumentException>(() => new OptionExtractorContext(new OptionString("test_arg_string"), null), "Value cannot be null. (Parameter 'commandDescriptor')");
#pragma warning restore CA1806 // Do not ignore method results
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [TestMethod]
        public void NullOrWhiteSpaceOptionStringshouldError()
        {
#pragma warning disable CA1806 // Do not ignore method results
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            TestHelper.AssertThrowsWithMessage<ArgumentException>(() => new OptionExtractorContext(new OptionString(null), command.Item1), "'raw' cannot be null or whitespace. (Parameter 'raw')");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            TestHelper.AssertThrowsWithMessage<ArgumentException>(() => new OptionExtractorContext(new OptionString("   "), command.Item1), "'raw' cannot be null or whitespace. (Parameter 'raw')");
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
            options.Extractor.OptionPrefix = "";
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            CommandDescriptor cmd = new("i1", "n1", "p1", "desc1", new OptionDescriptors(textHandler, new[] { new OptionDescriptor($"{prefix}key", System.ComponentModel.DataAnnotations.DataType.Text, "desc1") }));
            OptionExtractorContext context = new OptionExtractorContext(new OptionString($"{prefix}key=value"), cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Option);
            Assert.AreEqual($"{prefix}key", result.Option.Id);
            Assert.AreEqual($"value", result.Option.Value);
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
            options.Extractor.OptionPrefix = prefix;

            // Prefix are not allowed in option id
            CommandDescriptor cmd = new("i1", "n1", "p1", "desc1", new OptionDescriptors(textHandler, new[] { new OptionDescriptor($"{prefix}key", System.ComponentModel.DataAnnotations.DataType.Text, "desc1") }));
            OptionExtractorContext context = new(new OptionString($"{prefix}key1=value1"), cmd);

            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedOption, "The option is not supported. option=key1");
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
        public async Task PrefixInOptionValueShouldNotErrorAsync(string prefix)
        {
            options.Extractor.OptionPrefix = prefix;

            // Prefix are not allowed in option id
            CommandDescriptor cmd = new("i1", "n1", "p1", "desc1", new OptionDescriptors(textHandler, new[] { new OptionDescriptor($"key1", System.ComponentModel.DataAnnotations.DataType.Text, "desc1") }));
            OptionExtractorContext context = new(new OptionString($"{prefix}key1={prefix}value1{prefix}"), cmd);
            var result = await extractor.ExtractAsync(context);

            Assert.AreEqual("key1", result.Option.Id);
            Assert.AreEqual($"{prefix}value1{prefix}", result.Option.Value);
        }

        [TestMethod]
        public async Task UnicodeArgStringDoesNotStartWithSeparatorShouldExtractCorrectly()
        {
            options.Extractor.Separator = "स";
            options.Extractor.OptionPrefix = "ई";
            options.Extractor.OptionValueSeparator = "र";

            CommandDescriptor cmd = new("i1", "n1", "p1", "desc1", new OptionDescriptors(textHandler, new[] { new OptionDescriptor("पक्षी", System.ComponentModel.DataAnnotations.DataType.Text, "desc1") }));
            OptionExtractorContext context = new(new OptionString($"ईपक्षीरप्राणीप्रेम"), cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Option);
            Assert.AreEqual("पक्षी", result.Option.Id);
            Assert.AreEqual($"प्राणीप्रेम", result.Option.Value);
        }

        [TestMethod]
        public async Task UnicodeArgStringNoArgSeparatorForBooleanShouldNotError()
        {
            options.Extractor.Separator = "स";
            options.Extractor.OptionPrefix = "ई";
            options.Extractor.OptionValueSeparator = "र";

            CommandDescriptor cmd = new("i1", "n1", "p1", "desc1", new OptionDescriptors(textHandler, new[] { new OptionDescriptor("पक्षी", nameof(Boolean), "पक्षी वर्णन") }));
            OptionExtractorContext context = new(new OptionString("ईपक्षी"), cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Option);
            Assert.AreEqual("पक्षी", result.Option.Id);
            Assert.AreEqual("True", result.Option.Value);
        }

        [TestMethod]
        public async Task UnicodeArgStringNoArgSeparatorShouldErrorAsync()
        {
            options.Extractor.Separator = "स";
            options.Extractor.OptionPrefix = "ई";
            options.Extractor.OptionValueSeparator = "र";

            CommandDescriptor cmd = new("i1", "n1", "p1", "desc1", new OptionDescriptors(textHandler, new[] { new OptionDescriptor("पक्षी", System.ComponentModel.DataAnnotations.DataType.Text, "पक्षी वर्णन") }));
            OptionExtractorContext context = new(new OptionString("ईपक्षी"), cmd);

            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.InvalidOption, "The option value is missing. option_string=ईपक्षीर");
        }

        [TestMethod]
        public async Task UnicodeArgStringNoValueShouldNotErrorAsync()
        {
            options.Extractor.Separator = "स";
            options.Extractor.OptionPrefix = "ई";
            options.Extractor.OptionValueSeparator = "र";

            CommandDescriptor cmd = new("i1", "n1", "p1", "desc1", new OptionDescriptors(textHandler, new[] { new OptionDescriptor("पक्षी", System.ComponentModel.DataAnnotations.DataType.Text, "पक्षी वर्णन") }));
            OptionExtractorContext context = new(new OptionString("ईपक्षीर"), cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Option);
            Assert.AreEqual("पक्षी", result.Option.Id);
            Assert.AreEqual("", result.Option.Value);
        }

        [TestMethod]
        public async Task UnicodeArgStringStartsWithArgSeparatorShouldErrorAsync()
        {
            options.Extractor.Separator = "स";
            options.Extractor.OptionPrefix = "ई";
            options.Extractor.OptionValueSeparator = "र";

            CommandDescriptor cmd = new("i1", "n1", "p1", "desc1", new OptionDescriptors(textHandler, new[] { new OptionDescriptor("पक्षी", System.ComponentModel.DataAnnotations.DataType.Text, "पक्षी वर्णन") }));
            OptionExtractorContext context = new(new OptionString("रईपक्षीप्राणीप्रेम"), cmd);

            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.InvalidOption, "The option string is not valid. option_string=रईपक्षीप्राणीप्रेम");
        }

        [TestMethod]
        public async Task UnicodeArgStringStartsWithSeparatorShouldExtractCorrectly()
        {
            options.Extractor.Separator = "स";
            options.Extractor.OptionPrefix = "ई";
            options.Extractor.OptionValueSeparator = "र";

            CommandDescriptor cmd = new("i1", "n1", "p1", "desc1", new OptionDescriptors(textHandler, new[] { new OptionDescriptor("पक्षी", System.ComponentModel.DataAnnotations.DataType.Text, "पक्षी वर्णन") }));
            OptionExtractorContext context = new(new OptionString("सईपक्षीरप्राणीप्रेम"), cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Option);
            Assert.AreEqual("पक्षी", result.Option.Id);
            Assert.AreEqual($"प्राणीप्रेम", result.Option.Value);
        }

        [TestMethod]
        public async Task UnicodeArgStringValueNotWithinShouldNotErrorAsync()
        {
            options.Extractor.Separator = "स";
            options.Extractor.OptionPrefix = "ई";
            options.Extractor.OptionValueSeparator = "र";
            options.Extractor.OptionValueWithIn = "बी";

            CommandDescriptor cmd = new("i1", "n1", "p1", "desc1", new OptionDescriptors(textHandler, new[] { new OptionDescriptor("पक्षी", System.ComponentModel.DataAnnotations.DataType.Text, "पक्षी वर्णन") }));
            OptionExtractorContext context = new(new OptionString("ईपक्षीरप्राणीप्रेमबी"), cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Option);
            Assert.AreEqual("पक्षी", result.Option.Id);
            Assert.AreEqual($"प्राणीप्रेमबी", result.Option.Value);
        }

        [TestMethod]
        public async Task UnicodeArgStringValueWithinShouldNotErrorAsync()
        {
            options.Extractor.Separator = "स";
            options.Extractor.OptionPrefix = "ई";
            options.Extractor.OptionValueSeparator = "र";
            options.Extractor.OptionValueWithIn = "बी";

            CommandDescriptor cmd = new("i1", "n1", "p1", "desc1", new OptionDescriptors(textHandler, new[] { new OptionDescriptor("पक्षी", System.ComponentModel.DataAnnotations.DataType.Text, "पक्षी वर्णन") }));
            OptionExtractorContext context = new(new OptionString("ईपक्षीरबीप्राणीप्रेमबी"), cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Option);
            Assert.AreEqual("पक्षी", result.Option.Id);
            Assert.AreEqual($"प्राणीप्रेम", result.Option.Value);
        }

        [TestMethod]
        public async Task UnicodeArgStringWithMultiplePrefixAndSeparatorShouldNotErrorAsync()
        {
            options.Extractor.Separator = "स";
            options.Extractor.OptionPrefix = "ई";
            options.Extractor.OptionValueSeparator = "र";

            CommandDescriptor cmd = new("i1", "n1", "p1", "desc1", new OptionDescriptors(textHandler, new[] { new OptionDescriptor("पक्षी", System.ComponentModel.DataAnnotations.DataType.Text, "पक्षी वर्णन") }));
            OptionExtractorContext context = new(new OptionString("ससससससईईईईईईईईपक्षीरररररररररप्राणीप्रेमसससससस"), cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Option);
            Assert.AreEqual("पक्षी", result.Option.Id);
            Assert.AreEqual($"प्राणीप्रेम", result.Option.Value);
        }

        [TestMethod]
        public async Task UnicodePrefixInValueShouldNotErrorAsync()
        {
            options.Extractor.OptionPrefix = "माणू";

            CommandDescriptor cmd = new("i1", "n1", "p1", "desc1", new OptionDescriptors(textHandler, new[] { new OptionDescriptor("स", System.ComponentModel.DataAnnotations.DataType.Text, "पक्षी वर्णन") }));
            OptionExtractorContext context = new(new OptionString("माणूमाणूस=माणूसमास"), cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Option);
            Assert.AreEqual($"स", result.Option.Id);
            Assert.AreEqual($"माणूसमास", result.Option.Value);
        }

        [TestMethod]
        public async Task UsupportedArgumentShouldErrorAsync()
        {
            OptionExtractorContext context = new OptionExtractorContext(new OptionString("-invalid=value"), command.Item1);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedOption, "The option is not supported. option=invalid");
        }

        [DataTestMethod]
        [DataRow("key1_alias")]
        [DataRow("öö")]
        [DataRow("मासे")]
        [DataRow("女性")]
        public async Task ValidArgAliasOnlyShouldNotErrorAsync(string keyAlias)
        {
            options.Extractor.OptionAlias = true;
            options.Extractor.OptionPrefix = "-";
            options.Extractor.OptionAliasPrefix = "--";

            CommandDescriptor cmd = new("i1", "n1", "p1", "desc1", new OptionDescriptors(textHandler, new[] { new OptionDescriptor("key1", nameof(Boolean), "test desc") { Alias = keyAlias } }));
            OptionExtractorContext context = new(new OptionString($"--{keyAlias}", aliasPrefix: true, 0), cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Option);
            Assert.AreEqual("key1", result.Option.Id);
            Assert.AreEqual(System.ComponentModel.DataAnnotations.DataType.Custom, result.Option.DataType);
            Assert.AreEqual(nameof(Boolean), result.Option.CustomDataType);
            Assert.AreEqual("True", result.Option.Value);
        }

        [DataTestMethod]
        [DataRow("key1")]
        [DataRow("öö")]
        [DataRow("मासे")]
        [DataRow("女性")]
        public async Task ValidArgIdOnlyShouldNotErrorAsync(string key)
        {
            CommandDescriptor cmd = new("i1", "n1", "p1", "desc1", new OptionDescriptors(textHandler, new[] { new OptionDescriptor(key, nameof(Boolean), "test desc") }));
            OptionExtractorContext context = new OptionExtractorContext(new OptionString($"-{key}"), cmd);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Option);
            Assert.AreEqual(key, result.Option.Id);
            Assert.AreEqual(System.ComponentModel.DataAnnotations.DataType.Custom, result.Option.DataType);
            Assert.AreEqual(nameof(Boolean), result.Option.CustomDataType);
            Assert.AreEqual("True", result.Option.Value);
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
            options.Extractor.OptionValueWithIn = withIn;

            OptionExtractorContext context = new(new OptionString($"-key1=test string with {withIn} in between and end but not at start {withIn}"), command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Option);
            Assert.AreEqual($"key1", result.Option.Id);
            Assert.AreEqual($"test string with {withIn} in between and end but not at start {withIn}", result.Option.Value);
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
            options.Extractor.OptionValueWithIn = withIn;

            OptionExtractorContext context = new(new OptionString($"-key1={withIn}test string with {withIn} in between {withIn}"), command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Option);
            Assert.AreEqual($"key1", result.Option.Id);
            Assert.AreEqual($"test string with {withIn} in between ", result.Option.Value);
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
            options.Extractor.OptionValueWithIn = null;

            OptionExtractorContext context = new(new OptionString($"-key1={withIn}test string with {withIn} in between {withIn}"), command.Item1);
            var result = await extractor.ExtractAsync(context);
            Assert.IsNotNull(result.Option);
            Assert.AreEqual($"key1", result.Option.Id);
            Assert.AreEqual($"{withIn}test string with {withIn} in between {withIn}", result.Option.Value);
        }

        protected override void OnTestInitialize()
        {
            textHandler = new UnicodeTextHandler();
            command = MockCommands.NewCommandDefinition("id1", "name1", "prefix1", "desc1", MockCommands.TestOptionDescriptors, null, null);
            options = MockTerminalOptions.New();
            extractor = new OptionExtractor(textHandler, options, TestLogger.Create<OptionExtractor>());
        }

        private Tuple<CommandDescriptor, Command> command = null!;
        private OptionExtractor extractor = null!;
        private TerminalOptions options = null!;
        private ITextHandler textHandler = null!;
    }
}