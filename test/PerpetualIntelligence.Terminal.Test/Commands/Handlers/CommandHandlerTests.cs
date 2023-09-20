/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Terminal.Commands.Checkers;
using PerpetualIntelligence.Terminal.Commands.Extractors;
using PerpetualIntelligence.Terminal.Commands.Handlers.Mocks;
using PerpetualIntelligence.Terminal.Commands.Providers;
using PerpetualIntelligence.Terminal.Commands.Routers;
using PerpetualIntelligence.Terminal.Commands.Runners;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Events;
using PerpetualIntelligence.Terminal.Licensing;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Terminal.Runtime;
using PerpetualIntelligence.Test;
using PerpetualIntelligence.Test.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Handlers
{
    [TestClass]
    public class CommandHandlerTests : InitializerTests
    {
        public CommandHandlerTests() : base(TestLogger.Create<CommandHandlerTests>())
        {
        }

        [TestMethod]
        public async Task CheckerConfiguredButNotAddedToServiceCollectionShouldErrorAsync()
        {
            // Mock checker configured
            command.Item1.Checker = typeof(MockCommandCheckerInner);

            // No mock checker added to collection.
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>());
            using var newHost = hostBuilder.Build();

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            var newHandler = new CommandHandler(newHost.Services, licenseChecker, terminalOptions, TestLogger.Create<CommandHandler>());
            await TestHelper.AssertThrowsErrorExceptionAsync(() => newHandler.HandleAsync(commandContext), TerminalErrors.ServerError, "The command checker is not registered with service collection. command_name=name1 command_id=id1 checker=MockCommandCheckerInner");
        }

        [TestMethod]
        public async Task CheckerErrorShouldErrorHandler()
        {
            command.Item1.Checker = typeof(MockErrorCommandCheckerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => handler.HandleAsync(commandContext), "test_checker_error", "test_checker_error_desc");
        }

        [TestMethod]
        public async Task CheckerNotConfiguredShouldError()
        {
            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => handler.HandleAsync(commandContext), TerminalErrors.ServerError, "The command checker is not configured. command_name=name1 command_id=id1");
        }

        [TestMethod]
        public async Task Handler_Does_Process_And_Dispose_ResultAsync()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            // MockCommandRunnerInnerResult.ResultProcessed and MockCommandRunnerInnerResult.ResultDisposed are static
            // because result is not part of DI, its instance is created by the framework.
            try
            {
                Assert.IsFalse(MockCommandRunnerInnerResult.ResultProcessed);
                Assert.IsFalse(MockCommandRunnerInnerResult.ResultDisposed);

                CommandRoute commandRoute = new("test_id", "test_raw");
                ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

                CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
                await handler.HandleAsync(commandContext);

                Assert.IsTrue(MockCommandRunnerInnerResult.ResultProcessed);
                Assert.IsTrue(MockCommandRunnerInnerResult.ResultDisposed);
            }
            finally
            {
                MockCommandRunnerInnerResult.ResultProcessed = false;
                MockCommandRunnerInnerResult.ResultDisposed = false;
            }
        }

        [TestMethod]
        public async Task InvalidCheckerShouldError()
        {
            // Not a ICommandChecker
            command.Item1.Checker = typeof(MockNotCheckerOrRunner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => handler.HandleAsync(commandContext), TerminalErrors.ServerError, "The command checker is not valid. command_name=name1 command_id=id1 checker=MockNotCheckerOrRunner");
        }

        [TestMethod]
        public async Task InvalidRunnerShouldError()
        {
            // Make sure checker pass so runner can fail
            command.Item1.Checker = typeof(MockCommandCheckerInner);

            // Not a ICommandChecker
            command.Item1.Runner = typeof(MockNotCheckerOrRunner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => handler.HandleAsync(commandContext), TerminalErrors.ServerError, "The command runner delegate is not configured. command_name=name1 command_id=id1 runner=MockNotCheckerOrRunner");
        }

        [TestMethod]
        public async Task RunnerConfiguredButNotAddedToServiceCollectionShouldErrorAsync()
        {
            // Make sure checker pass so runner can fail. // No mock runner added to collection. Configure checker so
            // checker can pass
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureCheckerOnly);

            // Mock checker configured but not added to service collection
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            using IHost newHost = hostBuilder.Build();
            var newHandler = new CommandHandler(newHost.Services, licenseChecker, terminalOptions, TestLogger.Create<CommandHandler>());

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => newHandler.HandleAsync(commandContext), TerminalErrors.ServerError, "The command runner is not registered with service collection. command_name=name1 command_id=id1 runner=MockCommandRunnerInner");
        }

        [TestMethod]
        public async Task RunnerErrorShouldErrorHandler()
        {
            // Make sure checker pass so runner can fail
            command.Item1.Checker = typeof(MockCommandCheckerInner);

            command.Item1.Runner = typeof(MockErrorCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => handler.HandleAsync(commandContext), "test_runner_error", "test_runner_error_desc");
        }

        [TestMethod]
        public async Task HelpErrorShouldErrorHandler()
        {
            // Make sure checker pass so runner can fail
            helpCommand.Item1.Checker = typeof(MockCommandCheckerInner);
            helpCommand.Item1.Runner = typeof(MockErrorCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, helpCommand.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => handler.HandleAsync(commandContext), "test_runner_help_error", "test_runner_help_error_desc");
        }

        [TestMethod]
        public async Task HelpShouldBeCalledIfEnabledAndRequested()
        {
            helpCommand.Item1.Checker = typeof(MockCommandCheckerInner);
            helpCommand.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, helpCommand.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await handler.HandleAsync(commandContext);

            MockHelpProvider mockHelpProvider = (MockHelpProvider)host.Services.GetRequiredService<IHelpProvider>();
            mockHelpProvider.HelpCalled.Should().BeTrue();

            MockCommandRunnerInner commandRunnerInner = host.Services.GetRequiredService<MockCommandRunnerInner>();
            commandRunnerInner.DelegateHelpCalled.Should().BeTrue();
            commandRunnerInner.HelpCalled.Should().BeTrue();

            commandRunnerInner.DelegateRunCalled.Should().BeFalse();
            commandRunnerInner.RunCalled.Should().BeFalse();
        }

        [TestMethod]
        public async Task HelpShouldBeCalledAndReturnsNoProcessing()
        {
            helpCommand.Item1.Checker = typeof(MockCommandCheckerInner);
            helpCommand.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, helpCommand.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            var result = await handler.HandleAsync(commandContext);

            result.RunnerResult.Should().NotBeEquivalentTo(CommandRunnerResult.NoProcessing);

            result.RunnerResult.IsDisposed.Should().BeTrue();
            result.RunnerResult.IsProcessed.Should().BeTrue();
        }

        [TestMethod]
        public async Task RunShouldBeCalledIfHelpIsDisabled()
        {
            terminalOptions.Help.Disabled = true;

            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await handler.HandleAsync(commandContext);

            MockCommandRunnerInner commandRunnerInner = host.Services.GetRequiredService<MockCommandRunnerInner>();
            commandRunnerInner.DelegateHelpCalled.Should().BeFalse();
            commandRunnerInner.HelpCalled.Should().BeFalse();

            commandRunnerInner.DelegateRunCalled.Should().BeTrue();
            commandRunnerInner.RunCalled.Should().BeTrue();
        }

        [TestMethod]
        public async Task RunShouldBeCalled()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await handler.HandleAsync(commandContext);

            MockCommandRunnerInner commandRunnerInner = host.Services.GetRequiredService<MockCommandRunnerInner>();
            commandRunnerInner.DelegateHelpCalled.Should().BeFalse();
            commandRunnerInner.HelpCalled.Should().BeFalse();

            commandRunnerInner.DelegateRunCalled.Should().BeTrue();
            commandRunnerInner.RunCalled.Should().BeTrue();
        }

        [TestMethod]
        public async Task HelpShouldNotBeCalledIfDisabledAndRequested()
        {
            terminalOptions.Help.Disabled = true;

            helpCommand.Item1.Checker = typeof(MockCommandCheckerInner);
            helpCommand.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, helpCommand.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await handler.HandleAsync(commandContext);

            MockHelpProvider mockHelpProvider = (MockHelpProvider)host.Services.GetRequiredService<IHelpProvider>();
            mockHelpProvider.HelpCalled.Should().BeFalse();
        }

        [TestMethod]
        public async Task HelpShouldNotBeCalledIfEnabledAndNotRequested()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await handler.HandleAsync(commandContext);

            MockHelpProvider mockHelpProvider = (MockHelpProvider)host.Services.GetRequiredService<IHelpProvider>();
            mockHelpProvider.HelpCalled.Should().BeFalse();
        }

        [TestMethod]
        public async Task RunnerNotConfiguredShouldError()
        {
            // Make sure checker pass so runner can fail
            command.Item1.Checker = typeof(MockCommandCheckerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await TestHelper.AssertThrowsErrorExceptionAsync(() => handler.HandleAsync(commandContext), TerminalErrors.ServerError, "The command runner is not configured. command_name=name1 command_id=id1");
        }

        [TestMethod]
        public async Task ValidCheckerAndRunnerShouldCheckCorrectLicenseFeaturesAsync()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await handler.HandleAsync(commandContext);

            Assert.IsTrue(licenseChecker.Called);
            Assert.IsNotNull(licenseChecker.ContextCalled);
            Assert.AreEqual(license, licenseChecker.ContextCalled.License);
        }

        [TestMethod]
        public async Task ValidCheckerAndRunnerShouldAllowHandlerAsync()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            var result = await handler.HandleAsync(commandContext);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<CommandHandlerResult>(result);

            Assert.IsNotNull(result.RunnerResult);
            Assert.IsInstanceOfType<CommandRunnerResult>(result.RunnerResult);
        }

        [TestMethod]
        public async Task ValidGenericRunnerShouldAllowHandlerAsync()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockGenericCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            var result = await handler.HandleAsync(commandContext);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<CommandHandlerResult>(result);

            Assert.IsNotNull(result.CheckerResult);
            Assert.IsInstanceOfType<CommandCheckerResult>(result.CheckerResult);

            Assert.IsNotNull(result.RunnerResult);
            Assert.IsInstanceOfType<MockGenericCommandRunnerResult>(result.RunnerResult);
        }

        [TestMethod]
        public async Task ShouldCallCheckerEventIfConfigured()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesWithEventHandler);
            host = hostBuilder.Build();
            handler = new CommandHandler(host.Services, licenseChecker, terminalOptions, TestLogger.Create<CommandHandler>());

            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockGenericCommandRunnerInner);

            MockAsyncEventHandler asyncEventHandler = (MockAsyncEventHandler)host.Services.GetRequiredService<IAsyncEventHandler>();
            asyncEventHandler.BeforeCheckCalled.Should().Be(false);
            asyncEventHandler.AfterCheckCalled.Should().Be(false);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            var result = await handler.HandleAsync(commandContext);

            asyncEventHandler.BeforeCheckCalled.Should().Be(true);
            asyncEventHandler.AfterCheckCalled.Should().Be(true);
        }

        [TestMethod]
        public async Task ShouldCallRunnerEventIfConfigured()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesWithEventHandler);
            host = hostBuilder.Build();
            handler = new CommandHandler(host.Services, licenseChecker, terminalOptions, TestLogger.Create<CommandHandler>());

            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockGenericCommandRunnerInner);

            MockAsyncEventHandler asyncEventHandler = (MockAsyncEventHandler)host.Services.GetRequiredService<IAsyncEventHandler>();
            asyncEventHandler.BeforeRunCalled.Should().Be(false);
            asyncEventHandler.AfterRunCalled.Should().Be(false);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            var result = await handler.HandleAsync(commandContext);

            asyncEventHandler.BeforeRunCalled.Should().Be(true);
            asyncEventHandler.AfterRunCalled.Should().Be(true);
        }

        [TestMethod]
        public async Task ShouldNotCallAfterRunnerEventIfConfiguredWithAnError()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesWithEventHandler);
            host = hostBuilder.Build();
            handler = new CommandHandler(host.Services, licenseChecker, terminalOptions, TestLogger.Create<CommandHandler>());

            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockGenericCommandRunnerInner);

            MockAsyncEventHandler asyncEventHandler = (MockAsyncEventHandler)host.Services.GetRequiredService<IAsyncEventHandler>();
            asyncEventHandler.BeforeRunCalled.Should().BeFalse();
            asyncEventHandler.AfterRunCalled.Should().BeFalse();

            // Runner throws
            MockGenericCommandRunnerInner runner = host.Services.GetRequiredService<MockGenericCommandRunnerInner>();
            runner.ThrowException = true;

            try
            {
                CommandRoute commandRoute = new("test_id", "test_raw");
                ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

                CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
                var result = await handler.HandleAsync(commandContext);
            }
            catch (ErrorException eex)
            {
                eex.Error.ErrorCode.Should().Be("test_error");
                eex.Error.ErrorDescription.Should().Be("test_desc");
            }

            asyncEventHandler.BeforeRunCalled.Should().BeTrue();
            asyncEventHandler.AfterRunCalled.Should().BeFalse();
        }

        [TestMethod]
        public async Task ShouldNotCallAfterCheckerEventIfConfiguredWithAnError()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesWithEventHandler);
            host = hostBuilder.Build();
            handler = new CommandHandler(host.Services, licenseChecker, terminalOptions, TestLogger.Create<CommandHandler>());

            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockGenericCommandRunnerInner);

            MockAsyncEventHandler asyncEventHandler = (MockAsyncEventHandler)host.Services.GetRequiredService<IAsyncEventHandler>();
            asyncEventHandler.BeforeCheckCalled.Should().BeFalse();
            asyncEventHandler.AfterCheckCalled.Should().BeFalse();

            // Runner throws
            MockCommandCheckerInner checker = host.Services.GetRequiredService<MockCommandCheckerInner>();
            checker.ThrowException = true;

            try
            {
                CommandRoute commandRoute = new("test_id", "test_raw");
                ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

                CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
                var result = await handler.HandleAsync(commandContext);
            }
            catch (ErrorException eex)
            {
                eex.Error.ErrorCode.Should().Be("test_c_error");
                eex.Error.ErrorDescription.Should().Be("test_c_desc");
            }

            asyncEventHandler.BeforeCheckCalled.Should().BeTrue();
            asyncEventHandler.AfterCheckCalled.Should().BeFalse();

            asyncEventHandler.BeforeRunCalled.Should().BeFalse();
            asyncEventHandler.AfterRunCalled.Should().BeFalse();
        }

        protected override void OnTestInitialize()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServices);
            host = hostBuilder.Build();

            tokenSource = new CancellationTokenSource();
            terminalOptions = MockTerminalOptions.NewLegacyOptions();
            license = MockLicenses.TestLicense;
            licenseChecker = new MockLicenseCheckerInner();
            command = MockCommands.NewCommandDefinition("id1", "name1", "desc1", CommandType.SubCommand, CommandFlags.None);
            routingContext = new MockTerminalRoutingContext(new TerminalStartContext(new TerminalStartInfo(TerminalStartMode.Custom), tokenSource.Token));
            routerContext = new CommandRouterContext("test", routingContext);

            OptionDescriptors optionDescriptors = new(new UnicodeTextHandler(), new List<OptionDescriptor>()
            {
                new OptionDescriptor(terminalOptions.Help.OptionId, nameof(Boolean), "Help options", OptionFlags.None)
            });

            // This mocks the help requested
            Options options = new(new UnicodeTextHandler(), new Option[] { new Option(optionDescriptors.First().Value, true) });
            helpCommand = MockCommands.NewCommandDefinition("id2", "name2", "desc2", CommandType.SubCommand, CommandFlags.None, optionDescriptors, options: options);

            handler = new CommandHandler(host.Services, licenseChecker, terminalOptions, TestLogger.Create<CommandHandler>());
        }

        private void ConfigureCheckerOnly(IServiceCollection opt2)
        {
            opt2.AddTransient<MockCommandCheckerInner>();
        }

        private void ConfigureServices(IServiceCollection opt2)
        {
            opt2.AddSingleton<MockCommandCheckerInner>();
            opt2.AddSingleton<MockErrorCommandCheckerInner>();

            opt2.AddSingleton<MockCommandRunnerInner>();
            opt2.AddSingleton<MockErrorCommandRunnerInner>();
            opt2.AddSingleton<MockGenericCommandRunnerInner>();

            opt2.AddSingleton<MockNotCheckerOrRunner>();

            opt2.AddSingleton<IHelpProvider, MockHelpProvider>();
        }

        private void ConfigureServicesWithEventHandler(IServiceCollection opt2)
        {
            opt2.AddSingleton<MockCommandCheckerInner>();
            opt2.AddSingleton<MockErrorCommandCheckerInner>();

            opt2.AddSingleton<MockCommandRunnerInner>();
            opt2.AddSingleton<MockErrorCommandRunnerInner>();
            opt2.AddSingleton<MockGenericCommandRunnerInner>();

            opt2.AddSingleton<MockNotCheckerOrRunner>();

            opt2.AddSingleton<IAsyncEventHandler, MockAsyncEventHandler>();

            opt2.AddSingleton<IHelpProvider, MockHelpProvider>();
        }

        private Tuple<CommandDescriptor, Command> command = null!;
        private Tuple<CommandDescriptor, Command> helpCommand = null!;
        private CommandHandler handler = null!;
        private IHost host = null!;
        private License license = null!;
        private MockLicenseCheckerInner licenseChecker = null!;
        private TerminalOptions terminalOptions = null!;
        private CommandRouterContext routerContext = null!;
        private TerminalRoutingContext routingContext = null!;
        private CancellationTokenSource tokenSource = null!;
    }
}