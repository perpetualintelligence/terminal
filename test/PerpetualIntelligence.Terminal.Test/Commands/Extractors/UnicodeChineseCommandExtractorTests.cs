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

            Assert.IsNotNull(result.ParsedCommand.Command.Descriptor);
            Assert.AreEqual(CommandType.Group, result.ParsedCommand.Command.Descriptor.Type);

            Assert.IsNotNull(result.ParsedCommand);
            Assert.AreEqual("uc6", result.ParsedCommand.Command.Id);
            Assert.AreEqual("測試", result.ParsedCommand.Command.Name);
            Assert.AreEqual("示例分組命令", result.ParsedCommand.Command.Description);
            Assert.IsNull(result.ParsedCommand.Command.Options);
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

            Assert.IsNotNull(result.ParsedCommand.Command.Descriptor);
            Assert.AreEqual(CommandType.Root, result.ParsedCommand.Command.Descriptor.Type);

            Assert.IsNotNull(result.ParsedCommand);
            Assert.AreEqual("uc5", result.ParsedCommand.Command.Id);
            Assert.AreEqual("統一碼", result.ParsedCommand.Command.Name);
            Assert.AreEqual("示例根命令描述", result.ParsedCommand.Command.Description);
            Assert.IsNull(result.ParsedCommand.Command.Options);
        }

        [TestMethod]
        public async Task UnicodeSubCommand_Alias_Should_Extract_Correctly()
        {
            // 第一 is alias
            CommandExtractorContext context = new(new CommandRoute("id1", "統一碼 測試 打印 -第一 第一個值 --第二 --第三 第三個值 --第四 253.36"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.ParsedCommand.Command.Descriptor);

            Assert.IsNotNull(result.ParsedCommand);
            Assert.AreEqual("uc7", result.ParsedCommand.Command.Id);
            Assert.AreEqual("打印", result.ParsedCommand.Command.Name);
            Assert.AreEqual("測試命令", result.ParsedCommand.Command.Description);
            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            Assert.AreEqual(4, result.ParsedCommand.Command.Options.Count);

            AssertOption(result.ParsedCommand.Command.Options[0], "第一的", nameof(String), "第一個命令參數", "第一個值");
            AssertOption(result.ParsedCommand.Command.Options[1], "第二", nameof(Boolean), "第二個命令參數", true.ToString());
            AssertOption(result.ParsedCommand.Command.Options[2], "第三", nameof(String), "第三個命令參數", "第三個值");
            AssertOption(result.ParsedCommand.Command.Options[3], "第四", nameof(Double), "第四個命令參數", "253.36");
        }

        [TestMethod]
        public async Task UnicodeSubCommand_Should_Extract_Correctly()
        {
            CommandExtractorContext context = new(new CommandRoute("id1", "統一碼 測試 打印 --第一的 第一個值 --第二 --第三 第三個值 --第四 253.36"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.ParsedCommand.Command.Descriptor);

            Assert.IsNotNull(result.ParsedCommand);
            Assert.AreEqual("uc7", result.ParsedCommand.Command.Id);
            Assert.AreEqual("打印", result.ParsedCommand.Command.Name);
            Assert.AreEqual("測試命令", result.ParsedCommand.Command.Description);
            Assert.IsNotNull(result.ParsedCommand.Command.Options);
            Assert.AreEqual(4, result.ParsedCommand.Command.Options.Count);

            AssertOption(result.ParsedCommand.Command.Options[0], "第一的", nameof(String), "第一個命令參數", "第一個值");
            AssertOption(result.ParsedCommand.Command.Options[1], "第二", nameof(Boolean), "第二個命令參數", true.ToString());
            AssertOption(result.ParsedCommand.Command.Options[2], "第三", nameof(String), "第三個命令參數", "第三個值");
            AssertOption(result.ParsedCommand.Command.Options[3], "第四", nameof(Double), "第四個命令參數", "253.36");
        }

        protected override void OnTestInitialize()
        {
            options = MockTerminalOptions.NewAliasOptions();
            textHandler = new UnicodeTextHandler();
            optionExtractor = new OptionExtractor(textHandler, options, TestLogger.Create<OptionExtractor>());
            commandStore = new InMemoryCommandStore(MockCommands.UnicodeCommands);
            routeParser = new MockCommandRouteParser();
            optionExtractor = new OptionExtractor(textHandler, options, TestLogger.Create<OptionExtractor>());
            extractor = new CommandExtractor(routeParser);
        }

        private void AssertOption(Option arg, string name, string dataType, string description, object value)
        {
            Assert.AreEqual(arg.Id, name);
            Assert.AreEqual(arg.DataType, dataType);
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