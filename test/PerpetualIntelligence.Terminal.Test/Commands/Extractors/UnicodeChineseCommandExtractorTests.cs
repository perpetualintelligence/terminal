/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Commands.Providers;
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

            Assert.IsNotNull(result.Command.Descriptor);
            Assert.AreEqual("統一碼 測試", result.Command.Descriptor.Prefix);
            Assert.IsFalse(result.Command.Descriptor.IsRoot);
            Assert.IsTrue(result.Command.Descriptor.IsGroup);

            Assert.IsNotNull(result.Command);
            Assert.AreEqual("uc6", result.Command.Id);
            Assert.AreEqual("測試", result.Command.Name);
            Assert.AreEqual("示例分組命令", result.Command.Description);
            Assert.IsNull(result.Command.Options);
        }

        [TestMethod]
        public async Task UnicodeGroupedCommand_With_Imcomplete_Prefix_ShouldError()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "測試"));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedCommand, "The command prefix is not valid. prefix=測試");
        }

        [TestMethod]
        public async Task UnicodeRootCommand_Should_Extract_Correctly()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "統一碼"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.Command.Descriptor);
            Assert.AreEqual("統一碼", result.Command.Descriptor.Prefix);
            Assert.IsTrue(result.Command.Descriptor.IsRoot);

            Assert.IsNotNull(result.Command);
            Assert.AreEqual("uc5", result.Command.Id);
            Assert.AreEqual("統一碼", result.Command.Name);
            Assert.AreEqual("示例根命令描述", result.Command.Description);
            Assert.IsNull(result.Command.Options);
        }

        [TestMethod]
        public async Task UnicodeSubCommand_Alias_Should_Extract_Correctly()
        {
            // 第一 is alias
            CommandExtractorContext context = new(new CommandRoute("id1", "統一碼 測試 打印 -第一 第一個值 --第二 --第三 第三個值 --第四 253.36"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.Command.Descriptor);
            Assert.AreEqual("統一碼 測試 打印", result.Command.Descriptor.Prefix);

            Assert.IsNotNull(result.Command);
            Assert.AreEqual("uc7", result.Command.Id);
            Assert.AreEqual("打印", result.Command.Name);
            Assert.AreEqual("測試命令", result.Command.Description);
            Assert.IsNotNull(result.Command.Options);
            Assert.AreEqual(4, result.Command.Options.Count);

            AssertOption(result.Command.Options[0], "第一的", DataType.Text, "第一個命令參數", "第一個值");
            AssertOption(result.Command.Options[1], "第二", nameof(Boolean), "第二個命令參數", true.ToString());
            AssertOption(result.Command.Options[2], "第三", DataType.Text, "第三個命令參數", "第三個值");
            AssertOption(result.Command.Options[3], "第四", nameof(Double), "第四個命令參數", "253.36");
        }

        [TestMethod]
        public async Task UnicodeSubCommand_Default_Should_Extract_Correctly()
        {
            options.Extractor.DefaultOptionValue = true;
            options.Extractor.DefaultOption = true;

            // 第一 is required and has default value
            CommandExtractorContext context = new(new CommandRoute("id1", "統一碼 測試 打印 --第二 --第三 第三個值 --第四 253.36"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.Command.Descriptor);
            Assert.AreEqual("統一碼 測試 打印", result.Command.Descriptor.Prefix);

            Assert.IsNotNull(result.Command);
            Assert.AreEqual("uc7", result.Command.Id);
            Assert.AreEqual("打印", result.Command.Name);
            Assert.AreEqual("測試命令", result.Command.Description);
            Assert.IsNotNull(result.Command.Options);
            Assert.AreEqual(4, result.Command.Options.Count);

            AssertOption(result.Command.Options[0], "第二", nameof(Boolean), "第二個命令參數", true.ToString());
            AssertOption(result.Command.Options[1], "第三", DataType.Text, "第三個命令參數", "第三個值");
            AssertOption(result.Command.Options[2], "第四", nameof(Double), "第四個命令參數", "253.36");

            // Default added at the end
            AssertOption(result.Command.Options[3], "第一的", DataType.Text, "第一個命令參數", "默認值");
        }

        [TestMethod]
        public async Task UnicodeSubCommand_Should_Extract_Correctly()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "統一碼 測試 打印 --第一的 第一個值 --第二 --第三 第三個值 --第四 253.36"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.Command.Descriptor);
            Assert.AreEqual("統一碼 測試 打印", result.Command.Descriptor.Prefix);

            Assert.IsNotNull(result.Command);
            Assert.AreEqual("uc7", result.Command.Id);
            Assert.AreEqual("打印", result.Command.Name);
            Assert.AreEqual("測試命令", result.Command.Description);
            Assert.IsNotNull(result.Command.Options);
            Assert.AreEqual(4, result.Command.Options.Count);

            AssertOption(result.Command.Options[0], "第一的", DataType.Text, "第一個命令參數", "第一個值");
            AssertOption(result.Command.Options[1], "第二", nameof(Boolean), "第二個命令參數", true.ToString());
            AssertOption(result.Command.Options[2], "第三", DataType.Text, "第三個命令參數", "第三個值");
            AssertOption(result.Command.Options[3], "第四", nameof(Double), "第四個命令參數", "253.36");
        }

        protected override void OnTestInitialize()
        {
            options = MockTerminalOptions.NewOptions();
            textHandler = new UnicodeTextHandler();
            argExtractor = new OptionExtractor(textHandler, options, TestLogger.Create<OptionExtractor>());
            commands = new InMemoryCommandStore(textHandler, MockCommands.UnicodeCommands, options, TestLogger.Create<InMemoryCommandStore>());
            argExtractor = new OptionExtractor(textHandler, options, TestLogger.Create<OptionExtractor>());
            defaultArgValueProvider = new DefaultOptionValueProvider(textHandler);
            defaultArgProvider = new DefaultOptionProvider(options, TestLogger.Create<DefaultOptionProvider>());
            extractor = new CommandExtractor(commands, argExtractor, textHandler, options, TestLogger.Create<CommandExtractor>(), defaultArgProvider, defaultArgValueProvider);
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

        private OptionExtractor argExtractor = null!;
        private ICommandStoreHandler commands = null!;
        private IDefaultOptionProvider defaultArgProvider = null!;
        private IDefaultOptionValueProvider defaultArgValueProvider = null!;
        private CommandExtractor extractor = null!;
        private TerminalOptions options = null!;
        private ITextHandler textHandler = null!;
    }
}