/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Providers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Mocks;
using PerpetualIntelligence.Cli.Stores;
using PerpetualIntelligence.Cli.Stores.InMemory;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Extractors
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
            CommandExtractorContext context = new(new CommandString("統一碼 測試"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.CommandDescriptor);
            Assert.AreEqual("統一碼 測試", result.CommandDescriptor.Prefix);
            Assert.IsFalse(result.CommandDescriptor.IsRoot);
            Assert.IsTrue(result.CommandDescriptor.IsGroup);

            Assert.IsNotNull(result.Command);
            Assert.AreEqual("uc6", result.Command.Id);
            Assert.AreEqual("測試", result.Command.Name);
            Assert.AreEqual("示例分組命令", result.Command.Description);
            Assert.IsNull(result.Command.Arguments);
        }

        [TestMethod]
        public async Task UnicodeGroupedCommand_With_Imcomplete_Prefix_ShouldError()
        {
            CommandExtractorContext context = new(new CommandString("測試"));
            await TestHelper.AssertThrowsErrorExceptionAsync(() => extractor.ExtractAsync(context), Errors.UnsupportedCommand, "The command prefix is not valid. prefix=測試");
        }

        [TestMethod]
        public async Task UnicodeRootCommand_Should_Extract_Correctly()
        {
            CommandExtractorContext context = new(new CommandString("統一碼"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.CommandDescriptor);
            Assert.AreEqual("統一碼", result.CommandDescriptor.Prefix);
            Assert.IsTrue(result.CommandDescriptor.IsRoot);

            Assert.IsNotNull(result.Command);
            Assert.AreEqual("uc5", result.Command.Id);
            Assert.AreEqual("統一碼", result.Command.Name);
            Assert.AreEqual("示例根命令描述", result.Command.Description);
            Assert.IsNull(result.Command.Arguments);
        }

        [TestMethod]
        public async Task UnicodeSubCommand_Alias_Should_Extract_Correctly()
        {
            // 第一 is alias
            CommandExtractorContext context = new(new CommandString("統一碼 測試 打印 -第一 第一個值 --第二 --第三 第三個值 --第四 253.36"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.CommandDescriptor);
            Assert.AreEqual("統一碼 測試 打印", result.CommandDescriptor.Prefix);

            Assert.IsNotNull(result.Command);
            Assert.AreEqual("uc7", result.Command.Id);
            Assert.AreEqual("打印", result.Command.Name);
            Assert.AreEqual("測試命令", result.Command.Description);
            Assert.IsNotNull(result.Command.Arguments);
            Assert.AreEqual(4, result.Command.Arguments.Count);

            AssertArgument(result.Command.Arguments[0], "第一的", DataType.Text, "第一個命令參數", "第一個值");
            AssertArgument(result.Command.Arguments[1], "第二", nameof(Boolean), "第二個命令參數", true.ToString());
            AssertArgument(result.Command.Arguments[2], "第三", DataType.Text, "第三個命令參數", "第三個值");
            AssertArgument(result.Command.Arguments[3], "第四", nameof(Double), "第四個命令參數", "253.36");
        }

        [TestMethod]
        public async Task UnicodeSubCommand_Default_Should_Extract_Correctly()
        {
            options.Extractor.DefaultArgumentValue = true;
            options.Extractor.DefaultArgument = true;

            // 第一 is required and has default value
            CommandExtractorContext context = new(new CommandString("統一碼 測試 打印 --第二 --第三 第三個值 --第四 253.36"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.CommandDescriptor);
            Assert.AreEqual("統一碼 測試 打印", result.CommandDescriptor.Prefix);

            Assert.IsNotNull(result.Command);
            Assert.AreEqual("uc7", result.Command.Id);
            Assert.AreEqual("打印", result.Command.Name);
            Assert.AreEqual("測試命令", result.Command.Description);
            Assert.IsNotNull(result.Command.Arguments);
            Assert.AreEqual(4, result.Command.Arguments.Count);

            AssertArgument(result.Command.Arguments[0], "第二", nameof(Boolean), "第二個命令參數", true.ToString());
            AssertArgument(result.Command.Arguments[1], "第三", DataType.Text, "第三個命令參數", "第三個值");
            AssertArgument(result.Command.Arguments[2], "第四", nameof(Double), "第四個命令參數", "253.36");

            // Default added at the end
            AssertArgument(result.Command.Arguments[3], "第一的", DataType.Text, "第一個命令參數", "默認值");
        }

        [TestMethod]
        public async Task UnicodeSubCommand_Should_Extract_Correctly()
        {
            CommandExtractorContext context = new(new CommandString("統一碼 測試 打印 --第一的 第一個值 --第二 --第三 第三個值 --第四 253.36"));
            var result = await extractor.ExtractAsync(context);

            Assert.IsNotNull(result.CommandDescriptor);
            Assert.AreEqual("統一碼 測試 打印", result.CommandDescriptor.Prefix);

            Assert.IsNotNull(result.Command);
            Assert.AreEqual("uc7", result.Command.Id);
            Assert.AreEqual("打印", result.Command.Name);
            Assert.AreEqual("測試命令", result.Command.Description);
            Assert.IsNotNull(result.Command.Arguments);
            Assert.AreEqual(4, result.Command.Arguments.Count);

            AssertArgument(result.Command.Arguments[0], "第一的", DataType.Text, "第一個命令參數", "第一個值");
            AssertArgument(result.Command.Arguments[1], "第二", nameof(Boolean), "第二個命令參數", true.ToString());
            AssertArgument(result.Command.Arguments[2], "第三", DataType.Text, "第三個命令參數", "第三個值");
            AssertArgument(result.Command.Arguments[3], "第四", nameof(Double), "第四個命令參數", "253.36");
        }

        protected override void OnTestInitialize()
        {
            options = MockCliOptions.NewOptions();
            textHandler = new UnicodeTextHandler();
            argExtractor = new ArgumentExtractor(textHandler, options, TestLogger.Create<ArgumentExtractor>());
            commands = new InMemoryCommandStore(textHandler, MockCommands.UnicodeCommands, options, TestLogger.Create<InMemoryCommandStore>());
            argExtractor = new ArgumentExtractor(textHandler, options, TestLogger.Create<ArgumentExtractor>());
            defaultArgValueProvider = new DefaultArgumentValueProvider(textHandler);
            defaultArgProvider = new DefaultArgumentProvider(options, TestLogger.Create<DefaultArgumentProvider>());
            extractor = new CommandExtractor(commands, argExtractor, textHandler, options, TestLogger.Create<CommandExtractor>(), defaultArgProvider, defaultArgValueProvider);
        }

        private void AssertArgument(Argument arg, string name, DataType dataType, string description, object value)
        {
            Assert.AreEqual(arg.Id, name);
            Assert.AreEqual(arg.DataType, dataType);
            Assert.IsNull(arg.CustomDataType);
            Assert.AreEqual(arg.Description, description);
            Assert.AreEqual(arg.Value, value);
        }

        private void AssertArgument(Argument arg, string name, string customDataType, string description, object value)
        {
            Assert.AreEqual(arg.Id, name);
            Assert.AreEqual(arg.DataType, DataType.Custom);
            Assert.AreEqual(arg.CustomDataType, customDataType);
            Assert.AreEqual(arg.Description, description);
            Assert.AreEqual(arg.Value, value);
        }

        private ArgumentExtractor argExtractor = null!;
        private ICommandStoreHandler commands = null!;
        private IDefaultArgumentProvider defaultArgProvider = null!;
        private IDefaultArgumentValueProvider defaultArgValueProvider = null!;
        private CommandExtractor extractor = null!;
        private CliOptions options = null!;
        private ITextHandler textHandler = null!;
    }
}
