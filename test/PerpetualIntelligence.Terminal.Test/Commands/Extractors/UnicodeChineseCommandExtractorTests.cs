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
    public class UnicodeCommandExtractorTests : InitializerTests
    {
        public UnicodeCommandExtractorTests() : base(TestLogger.Create<CommandExtractorTests>())
        {
        }

        [TestMethod]
        public async Task UnicodeGroupedCommand_Should_Extract_Correctly()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "統一碼 測試"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.ExtractedCommand.Command.Descriptor);
            Assert.AreEqual("統一碼 測試", result.ExtractedCommand.Command.Descriptor.Prefix);
            Assert.IsFalse(result.ExtractedCommand.Command.Descriptor.IsRoot);
            Assert.IsTrue(result.ExtractedCommand.Command.Descriptor.IsGroup);

            Assert.IsNotNull(result.ExtractedCommand);
            Assert.AreEqual("uc6", result.ExtractedCommand.Command.Id);
            Assert.AreEqual("測試", result.ExtractedCommand.Command.Name);
            Assert.AreEqual("示例分組命令", result.ExtractedCommand.Command.Description);
            Assert.IsNull(result.ExtractedCommand.Command.Options);
        }

        [TestMethod]
        public async Task UnicodeGroupedCommand_With_Incomplete_Prefix_ShouldError()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "測試"));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), TerminalErrors.UnsupportedCommand, "The command prefix is not valid. prefix=測試");
        }

        [TestMethod]
        public async Task UnicodeRootCommand_Should_Extract_Correctly()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "統一碼"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.ExtractedCommand.Command.Descriptor);
            Assert.AreEqual("統一碼", result.ExtractedCommand.Command.Descriptor.Prefix);
            Assert.IsTrue(result.ExtractedCommand.Command.Descriptor.IsRoot);

            Assert.IsNotNull(result.ExtractedCommand);
            Assert.AreEqual("uc5", result.ExtractedCommand.Command.Id);
            Assert.AreEqual("統一碼", result.ExtractedCommand.Command.Name);
            Assert.AreEqual("示例根命令描述", result.ExtractedCommand.Command.Description);
            Assert.IsNull(result.ExtractedCommand.Command.Options);
        }

        [TestMethod]
        public async Task UnicodeSubCommand_Alias_Should_Extract_Correctly()
        {
            // 第一 is alias
            CommandExtractorContext context = new(new CommandRoute("id1", "統一碼 測試 打印 -第一 第一個值 --第二 --第三 第三個值 --第四 253.36"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.ExtractedCommand.Command.Descriptor);
            Assert.AreEqual("統一碼 測試 打印", result.ExtractedCommand.Command.Descriptor.Prefix);

            Assert.IsNotNull(result.ExtractedCommand);
            Assert.AreEqual("uc7", result.ExtractedCommand.Command.Id);
            Assert.AreEqual("打印", result.ExtractedCommand.Command.Name);
            Assert.AreEqual("測試命令", result.ExtractedCommand.Command.Description);
            Assert.IsNotNull(result.ExtractedCommand.Command.Options);
            Assert.AreEqual(4, result.ExtractedCommand.Command.Options.Count);

            AssertOption(result.ExtractedCommand.Command.Options[0], "第一的", DataType.Text, "第一個命令參數", "第一個值");
            AssertOption(result.ExtractedCommand.Command.Options[1], "第二", nameof(Boolean), "第二個命令參數", true.ToString());
            AssertOption(result.ExtractedCommand.Command.Options[2], "第三", DataType.Text, "第三個命令參數", "第三個值");
            AssertOption(result.ExtractedCommand.Command.Options[3], "第四", nameof(Double), "第四個命令參數", "253.36");
        }

        [TestMethod]
        public async Task UnicodeSubCommand_Should_Extract_Correctly()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "統一碼 測試 打印 --第一的 第一個值 --第二 --第三 第三個值 --第四 253.36"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.ExtractedCommand.Command.Descriptor);
            Assert.AreEqual("統一碼 測試 打印", result.ExtractedCommand.Command.Descriptor.Prefix);

            Assert.IsNotNull(result.ExtractedCommand);
            Assert.AreEqual("uc7", result.ExtractedCommand.Command.Id);
            Assert.AreEqual("打印", result.ExtractedCommand.Command.Name);
            Assert.AreEqual("測試命令", result.ExtractedCommand.Command.Description);
            Assert.IsNotNull(result.ExtractedCommand.Command.Options);
            Assert.AreEqual(4, result.ExtractedCommand.Command.Options.Count);

            AssertOption(result.ExtractedCommand.Command.Options[0], "第一的", DataType.Text, "第一個命令參數", "第一個值");
            AssertOption(result.ExtractedCommand.Command.Options[1], "第二", nameof(Boolean), "第二個命令參數", true.ToString());
            AssertOption(result.ExtractedCommand.Command.Options[2], "第三", DataType.Text, "第三個命令參數", "第三個值");
            AssertOption(result.ExtractedCommand.Command.Options[3], "第四", nameof(Double), "第四個命令參數", "253.36");
        }

        protected override void OnTestInitialize()
        {
            options = MockTerminalOptions.NewAliasOptions();
            textHandler = new UnicodeTextHandler();
            optionExtractor = new OptionExtractor(textHandler, options, TestLogger.Create<OptionExtractor>());
            commandStore = new InMemoryCommandStore(textHandler, MockCommands.UnicodeCommands, options, TestLogger.Create<InMemoryCommandStore>());
            routeParser = new MockCommandRouteParser();
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