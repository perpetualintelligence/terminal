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
    public class UnicodeHindiCommandExtractorTests : InitializerTests
    {
        public UnicodeHindiCommandExtractorTests() : base(TestLogger.Create<CommandExtractorTests>())
        {
        }

        [TestMethod]
        public async Task UnicodeGroupedCommand_Should_Extract_Correctly()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "यूनिकोड परीक्षण"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.ExtractedCommand.Command.Descriptor);
            Assert.AreEqual("यूनिकोड परीक्षण", result.ExtractedCommand.Command.Descriptor.Prefix);
            Assert.IsFalse(result.ExtractedCommand.Command.Descriptor.IsRoot);
            Assert.IsTrue(result.ExtractedCommand.Command.Descriptor.IsGroup);

            Assert.IsNotNull(result.ExtractedCommand);
            Assert.AreEqual("uc2", result.ExtractedCommand.Command.Id);
            Assert.AreEqual("परीक्षण", result.ExtractedCommand.Command.Name);
            Assert.AreEqual("यूनिकोड समूहीकृत कमांड", result.ExtractedCommand.Command.Description);
            Assert.IsNull(result.ExtractedCommand.Command.Options);
        }

        [TestMethod]
        public async Task UnicodeGroupedCommand_With_Incomplete_Prefix_ShouldError()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "परीक्षण"));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), TerminalErrors.UnsupportedCommand, "The command prefix is not valid. prefix=परीक्षण");
        }

        [TestMethod]
        public async Task UnicodeRootCommand_Should_Extract_Correctly()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "यूनिकोड"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.ExtractedCommand.Command.Descriptor);
            Assert.AreEqual("यूनिकोड", result.ExtractedCommand.Command.Descriptor.Prefix);
            Assert.IsTrue(result.ExtractedCommand.Command.Descriptor.IsRoot);

            Assert.IsNotNull(result.ExtractedCommand);
            Assert.AreEqual("uc1", result.ExtractedCommand.Command.Id);
            Assert.AreEqual("यूनिकोड", result.ExtractedCommand.Command.Name);
            Assert.AreEqual("यूनिकोड रूट कमांड", result.ExtractedCommand.Command.Description);
            Assert.IsNull(result.ExtractedCommand.Command.Options);
        }

        [TestMethod]
        public async Task UnicodeSubCommand_Should_Extract_Correctly()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "यूनिकोड परीक्षण प्रिंट --एक पहला मूल्य --दो --तीन तीसरा मूल्य --चार 253.36"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.ExtractedCommand.Command.Descriptor);
            Assert.AreEqual("यूनिकोड परीक्षण प्रिंट", result.ExtractedCommand.Command.Descriptor.Prefix);

            Assert.IsNotNull(result.ExtractedCommand);
            Assert.AreEqual("uc3", result.ExtractedCommand.Command.Id);
            Assert.AreEqual("प्रिंट", result.ExtractedCommand.Command.Name);
            Assert.AreEqual("प्रिंट कमांड", result.ExtractedCommand.Command.Description);
            Assert.IsNotNull(result.ExtractedCommand.Command.Options);
            Assert.AreEqual(4, result.ExtractedCommand.Command.Options.Count);

            AssertOption(result.ExtractedCommand.Command.Options[0], "एक", DataType.Text, "पहला तर्क", "पहला मूल्य");
            AssertOption(result.ExtractedCommand.Command.Options[1], "दो", nameof(Boolean), "दूसरा तर्क", true.ToString());
            AssertOption(result.ExtractedCommand.Command.Options[2], "तीन", DataType.Text, "तीसरा तर्क", "तीसरा मूल्य");
            AssertOption(result.ExtractedCommand.Command.Options[3], "चार", nameof(Double), "चौथा तर्क", "253.36");
        }

        [TestMethod]
        public async Task UnicodeSubCommand_Alias_Should_Extract_Correctly()
        {
            // एकहै and चारहै are alias
            CommandExtractorContext context = new(new CommandRoute("id1", "यूनिकोड परीक्षण प्रिंट -एकहै पहला मूल्य --दो --तीन तीसरा मूल्य -चारहै 253.36"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.ExtractedCommand.Command.Descriptor);
            Assert.AreEqual("यूनिकोड परीक्षण प्रिंट", result.ExtractedCommand.Command.Descriptor.Prefix);

            Assert.IsNotNull(result.ExtractedCommand);
            Assert.AreEqual("uc3", result.ExtractedCommand.Command.Id);
            Assert.AreEqual("प्रिंट", result.ExtractedCommand.Command.Name);
            Assert.AreEqual("प्रिंट कमांड", result.ExtractedCommand.Command.Description);
            Assert.IsNotNull(result.ExtractedCommand.Command.Options);
            Assert.AreEqual(4, result.ExtractedCommand.Command.Options.Count);

            AssertOption(result.ExtractedCommand.Command.Options[0], "एक", DataType.Text, "पहला तर्क", "पहला मूल्य");
            AssertOption(result.ExtractedCommand.Command.Options[1], "दो", nameof(Boolean), "दूसरा तर्क", true.ToString());
            AssertOption(result.ExtractedCommand.Command.Options[2], "तीन", DataType.Text, "तीसरा तर्क", "तीसरा मूल्य");
            AssertOption(result.ExtractedCommand.Command.Options[3], "चार", nameof(Double), "चौथा तर्क", "253.36");
        }

        protected override void OnTestInitialize()
        {
            options = MockTerminalOptions.NewAliasOptions();
            textHandler = new UnicodeTextHandler();
            optionExtractor = new OptionExtractor(textHandler, options, TestLogger.Create<OptionExtractor>());
            routeParser = new MockCommandRouteParser();
            commandStore = new InMemoryCommandStore(textHandler, MockCommands.UnicodeCommands, options, TestLogger.Create<InMemoryCommandStore>());
            optionExtractor = new OptionExtractor(textHandler, options, TestLogger.Create<OptionExtractor>());
            extractor = new CommandExtractor(routeParser, commandStore, optionExtractor, textHandler, options, TestLogger.Create<CommandExtractor>());
        }

        private void AssertOption(Option arg, string name, DataType dataType, string description, object value)
        {
            Assert.AreEqual(arg.Id, name);
            Assert.AreEqual(arg.DataType, dataType);
            Assert.IsNull(arg.CustomDataType);
            Assert.AreEqual(arg.Description, description);
            Assert.AreEqual(arg.Value, value);
        }

        private void AssertOption(Option arg, string name, string customDataType, string description, object value)
        {
            Assert.AreEqual(arg.Id, name);
            Assert.AreEqual(arg.DataType, DataType.Custom);
            Assert.AreEqual(arg.CustomDataType, customDataType);
            Assert.AreEqual(arg.Description, description);
            Assert.AreEqual(arg.Value, value);
        }

        private OptionExtractor optionExtractor = null!;
        private ICommandRouteParser routeParser = null!;
        private ICommandStoreHandler commandStore = null!;
        private CommandExtractor extractor = null!;
        private TerminalOptions options = null!;
        private ITextHandler textHandler = null!;
    }
}