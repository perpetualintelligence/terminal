/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Handlers.Mocks;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Commands.Providers;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Events;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using OneImlx.Test.FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Commands.Handlers
{
    public class CommandHandlerTests : IAsyncLifetime
    {
        [Fact]
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
            var newHandler = new CommandHandler(newHost.Services, licenseChecker, terminalOptions, new LoggerFactory().CreateLogger<CommandHandler>());

            Func<Task> func = () => newHandler.HandleCommandAsync(commandContext);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.ServerError).WithErrorDescription("The command checker is not registered with service collection. command_name=name1 command_id=id1 checker=MockCommandCheckerInner");
        }

        [Fact]
        public async Task CheckerErrorShouldErrorHandler()
        {
            command.Item1.Checker = typeof(MockErrorCommandCheckerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            Func<Task> func = () => handler.HandleCommandAsync(commandContext);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode("test_checker_error").WithErrorDescription("test_checker_error_desc");
        }

        [Fact]
        public async Task CheckerNotConfiguredShouldError()
        {
            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            Func<Task> func = () => handler.HandleCommandAsync(commandContext);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.ServerError).WithErrorDescription("The command checker is not configured. command_name=name1 command_id=id1");
        }

        [Fact]
        public async Task Handler_Does_Process_And_Dispose_ResultAsync()
        {
            MockCommandRunnerInnerResult tempResult = new ();
            tempResult.ResultProcessed.Should().BeFalse();
            tempResult.ResultProcessed.Should().BeFalse();

            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            var result = await handler.HandleCommandAsync(commandContext);

            MockCommandRunnerInnerResult runnerResult = (MockCommandRunnerInnerResult)result.RunnerResult;
            runnerResult.ResultProcessed.Should().BeTrue();
            runnerResult.ResultDisposed.Should().BeTrue();
        }

        [Fact]
        public async Task InvalidCheckerShouldError()
        {
            // Not a ICommandChecker
            command.Item1.Checker = typeof(MockNotCheckerOrRunner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            Func<Task> func = () => handler.HandleCommandAsync(commandContext);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.ServerError).WithErrorDescription("The command checker is not valid. command_name=name1 command_id=id1 checker=MockNotCheckerOrRunner");
        }

        [Fact]
        public async Task InvalidRunnerShouldError()
        {
            // Make sure checker pass so runner can fail
            command.Item1.Checker = typeof(MockCommandCheckerInner);

            // Not a ICommandChecker
            command.Item1.Runner = typeof(MockNotCheckerOrRunner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            Func<Task> func = () => handler.HandleCommandAsync(commandContext);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.ServerError).WithErrorDescription("The command runner delegate is not configured. command_name=name1 command_id=id1 runner=MockNotCheckerOrRunner");
        }

        [Fact]
        public async Task RunnerConfiguredButNotAddedToServiceCollectionShouldErrorAsync()
        {
            // Make sure checker pass so runner can fail. // No mock runner added to collection. Configure checker so
            // checker can pass
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureCheckerOnly);

            // Mock checker configured but not added to service collection
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            using IHost newHost = hostBuilder.Build();
            var newHandler = new CommandHandler(newHost.Services, licenseChecker, terminalOptions, new LoggerFactory().CreateLogger<CommandHandler>());

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            Func<Task> func = () => newHandler.HandleCommandAsync(commandContext);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.ServerError).WithErrorDescription("The command runner is not registered with service collection. command_name=name1 command_id=id1 runner=MockCommandRunnerInner");
        }

        [Fact]
        public async Task RunnerErrorShouldErrorHandler()
        {
            // Make sure checker pass so runner can fail
            command.Item1.Checker = typeof(MockCommandCheckerInner);

            command.Item1.Runner = typeof(MockErrorCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            Func<Task> func = () => handler.HandleCommandAsync(commandContext);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode("test_runner_error").WithErrorDescription("test_runner_error_desc");
        }

        [Fact]
        public async Task HelpErrorShouldErrorHandler()
        {
            // Make sure checker pass so runner can fail
            helpIdCommand.Item1.Checker = typeof(MockCommandCheckerInner);
            helpIdCommand.Item1.Runner = typeof(MockErrorCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, helpIdCommand.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            Func<Task> func = () => handler.HandleCommandAsync(commandContext);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode("test_runner_help_error").WithErrorDescription("test_runner_help_error_desc");
        }

        [Fact]
        public async Task RunHelpIdShouldSkipCommandChecker()
        {
            helpIdCommand.Item1.Checker = typeof(MockCommandCheckerInner);
            helpIdCommand.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, helpIdCommand.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await handler.HandleCommandAsync(commandContext);

            MockHelpProvider mockHelpProvider = (MockHelpProvider)host.Services.GetRequiredService<IHelpProvider>();
            mockHelpProvider.HelpCalled.Should().BeTrue();

            MockCommandCheckerInner commandCheckerInner = host.Services.GetRequiredService<MockCommandCheckerInner>();
            commandCheckerInner.Called.Should().BeFalse();
        }

        [Fact]
        public async Task RunHelpAliasShouldSkipCommandChecker()
        {
            helpAliasCommand.Item1.Checker = typeof(MockCommandCheckerInner);
            helpAliasCommand.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, helpAliasCommand.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await handler.HandleCommandAsync(commandContext);

            MockHelpProvider mockHelpProvider = (MockHelpProvider)host.Services.GetRequiredService<IHelpProvider>();
            mockHelpProvider.HelpCalled.Should().BeTrue();

            MockCommandCheckerInner commandCheckerInner = host.Services.GetRequiredService<MockCommandCheckerInner>();
            commandCheckerInner.Called.Should().BeFalse();
        }

        [Fact]
        public async Task HelpShouldBeCalledIfEnabledAndRequested()
        {
            helpIdCommand.Item1.Checker = typeof(MockCommandCheckerInner);
            helpIdCommand.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, helpIdCommand.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await handler.HandleCommandAsync(commandContext);

            MockHelpProvider mockHelpProvider = (MockHelpProvider)host.Services.GetRequiredService<IHelpProvider>();
            mockHelpProvider.HelpCalled.Should().BeTrue();

            MockCommandRunnerInner commandRunnerInner = host.Services.GetRequiredService<MockCommandRunnerInner>();
            commandRunnerInner.DelegateHelpCalled.Should().BeTrue();
            commandRunnerInner.HelpCalled.Should().BeTrue();

            commandRunnerInner.DelegateRunCalled.Should().BeFalse();
            commandRunnerInner.RunCalled.Should().BeFalse();
        }

        [Fact]
        public async Task HelpShouldBeCalledAndReturnsNoProcessing()
        {
            helpIdCommand.Item1.Checker = typeof(MockCommandCheckerInner);
            helpIdCommand.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, helpIdCommand.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            var result = await handler.HandleCommandAsync(commandContext);

            result.RunnerResult.Should().NotBeEquivalentTo(CommandRunnerResult.NoProcessing);

            result.RunnerResult.IsDisposed.Should().BeTrue();
            result.RunnerResult.IsProcessed.Should().BeTrue();
        }

        [Fact]
        public async Task RunShouldBeCalledIfHelpIsDisabled()
        {
            terminalOptions.Help.Disabled = true;

            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await handler.HandleCommandAsync(commandContext);

            MockCommandRunnerInner commandRunnerInner = host.Services.GetRequiredService<MockCommandRunnerInner>();
            commandRunnerInner.DelegateHelpCalled.Should().BeFalse();
            commandRunnerInner.HelpCalled.Should().BeFalse();

            commandRunnerInner.DelegateRunCalled.Should().BeTrue();
            commandRunnerInner.RunCalled.Should().BeTrue();
        }

        [Fact]
        public async Task RunShouldBeCalled()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await handler.HandleCommandAsync(commandContext);

            MockCommandRunnerInner commandRunnerInner = host.Services.GetRequiredService<MockCommandRunnerInner>();
            commandRunnerInner.DelegateHelpCalled.Should().BeFalse();
            commandRunnerInner.HelpCalled.Should().BeFalse();

            commandRunnerInner.DelegateRunCalled.Should().BeTrue();
            commandRunnerInner.RunCalled.Should().BeTrue();
        }

        [Fact]
        public async Task HelpShouldNotBeCalledIfDisabledAndRequested()
        {
            terminalOptions.Help.Disabled = true;

            helpIdCommand.Item1.Checker = typeof(MockCommandCheckerInner);
            helpIdCommand.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, helpIdCommand.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await handler.HandleCommandAsync(commandContext);

            MockHelpProvider mockHelpProvider = (MockHelpProvider)host.Services.GetRequiredService<IHelpProvider>();
            mockHelpProvider.HelpCalled.Should().BeFalse();
        }

        [Fact]
        public async Task HelpShouldNotBeCalledIfEnabledAndNotRequested()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await handler.HandleCommandAsync(commandContext);

            MockHelpProvider mockHelpProvider = (MockHelpProvider)host.Services.GetRequiredService<IHelpProvider>();
            mockHelpProvider.HelpCalled.Should().BeFalse();
        }

        [Fact]
        public async Task RunnerNotConfiguredShouldError()
        {
            // Make sure checker pass so runner can fail
            command.Item1.Checker = typeof(MockCommandCheckerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            Func<Task> func = () => handler.HandleCommandAsync(commandContext);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode(TerminalErrors.ServerError).WithErrorDescription("The command runner is not configured. command_name=name1 command_id=id1");
        }

        [Fact]
        public async Task ValidCheckerAndRunnerShouldCheckCorrectLicenseFeaturesAsync()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await handler.HandleCommandAsync(commandContext);

            licenseChecker.Called.Should().BeTrue();
            licenseChecker.ContextCalled.Should().NotBeNull();
            licenseChecker.ContextCalled!.License.Should().BeSameAs(license);
        }

        [Fact]
        public async Task ValidCheckerAndRunnerShouldAllowHandlerAsync()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            var result = await handler.HandleCommandAsync(commandContext);
            result.Should().NotBeNull();
            result.Should().BeOfType<CommandHandlerResult>();

            result.RunnerResult.Should().NotBeNull();
            result.RunnerResult.Should().BeOfType<MockCommandRunnerInnerResult>();
        }

        [Fact]
        public async Task ValidGenericRunnerShouldAllowHandlerAsync()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockGenericCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            var result = await handler.HandleCommandAsync(commandContext);
            result.Should().NotBeNull();
            result.Should().BeOfType<CommandHandlerResult>();

            result.CheckerResult.Should().NotBeNull();
            result.CheckerResult.Should().BeOfType<CommandCheckerResult>();

            result.RunnerResult.Should().NotBeNull();
            result.RunnerResult.Should().BeOfType<MockGenericCommandRunnerResult>();
        }

        [Fact]
        public async Task ShouldCallCheckerEventIfConfigured()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesWithEventHandler);
            host = hostBuilder.Build();
            handler = new CommandHandler(host.Services, licenseChecker, terminalOptions, new LoggerFactory().CreateLogger<CommandHandler>());

            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockGenericCommandRunnerInner);

            MockAsyncEventHandler asyncEventHandler = (MockAsyncEventHandler)host.Services.GetRequiredService<ITerminalEventHandler>();
            asyncEventHandler.BeforeCheckCalled.Should().Be(false);
            asyncEventHandler.AfterCheckCalled.Should().Be(false);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            var result = await handler.HandleCommandAsync(commandContext);

            asyncEventHandler.BeforeCheckCalled.Should().Be(true);
            asyncEventHandler.AfterCheckCalled.Should().Be(true);
        }

        [Fact]
        public async Task ShouldCallRunnerEventIfConfigured()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesWithEventHandler);
            host = hostBuilder.Build();
            handler = new CommandHandler(host.Services, licenseChecker, terminalOptions, new LoggerFactory().CreateLogger<CommandHandler>());

            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockGenericCommandRunnerInner);

            MockAsyncEventHandler asyncEventHandler = (MockAsyncEventHandler)host.Services.GetRequiredService<ITerminalEventHandler>();
            asyncEventHandler.BeforeRunCalled.Should().Be(false);
            asyncEventHandler.AfterRunCalled.Should().Be(false);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            var result = await handler.HandleCommandAsync(commandContext);

            asyncEventHandler.BeforeRunCalled.Should().Be(true);
            asyncEventHandler.AfterRunCalled.Should().Be(true);
        }

        [Fact]
        public async Task ShouldNotCallAfterRunnerEventIfConfiguredWithAnError()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesWithEventHandler);
            host = hostBuilder.Build();
            handler = new CommandHandler(host.Services, licenseChecker, terminalOptions, new LoggerFactory().CreateLogger<CommandHandler>());

            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockGenericCommandRunnerInner);

            MockAsyncEventHandler asyncEventHandler = (MockAsyncEventHandler)host.Services.GetRequiredService<ITerminalEventHandler>();
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
                var result = await handler.HandleCommandAsync(commandContext);
            }
            catch (TerminalException eex)
            {
                eex.Error.ErrorCode.Should().Be("test_error");
                eex.Error.ErrorDescription.Should().Be("test_desc");
            }

            asyncEventHandler.BeforeRunCalled.Should().BeTrue();
            asyncEventHandler.AfterRunCalled.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldNotCallAfterCheckerEventIfConfiguredWithAnError()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServicesWithEventHandler);
            host = hostBuilder.Build();
            handler = new CommandHandler(host.Services, licenseChecker, terminalOptions, new LoggerFactory().CreateLogger<CommandHandler>());

            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockGenericCommandRunnerInner);

            MockAsyncEventHandler asyncEventHandler = (MockAsyncEventHandler)host.Services.GetRequiredService<ITerminalEventHandler>();
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
                var result = await handler.HandleCommandAsync(commandContext);
            }
            catch (TerminalException eex)
            {
                eex.Error.ErrorCode.Should().Be("test_c_error");
                eex.Error.ErrorDescription.Should().Be("test_c_desc");
            }

            asyncEventHandler.BeforeCheckCalled.Should().BeTrue();
            asyncEventHandler.AfterCheckCalled.Should().BeFalse();

            asyncEventHandler.BeforeRunCalled.Should().BeFalse();
            asyncEventHandler.AfterRunCalled.Should().BeFalse();
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

            opt2.AddSingleton<ITerminalEventHandler, MockAsyncEventHandler>();

            opt2.AddSingleton<IHelpProvider, MockHelpProvider>();
        }

        public Task InitializeAsync()
        {
            var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>()).ConfigureServices(ConfigureServices);
            host = hostBuilder.Build();

            terminalTokenSource = new CancellationTokenSource();
            commandTokenSource = new CancellationTokenSource();
            terminalOptions = MockTerminalOptions.NewLegacyOptions();
            license = MockLicenses.TestLicense;
            licenseChecker = new MockLicenseCheckerInner();
            command = MockCommands.NewCommandDefinition("id1", "name1", "desc1", CommandType.SubCommand, CommandFlags.None);
            routingContext = new MockTerminalRouterContext(new TerminalStartContext(TerminalStartMode.Custom, terminalTokenSource.Token, commandTokenSource.Token));
            routerContext = new CommandRouterContext("test", routingContext);

            // This mocks the help id request
            OptionDescriptors helpIdOptionDescriptors = new(new TerminalUnicodeTextHandler(), new List<OptionDescriptor>()
            {
                new OptionDescriptor(terminalOptions.Help.OptionId, nameof(Boolean), "Help options", OptionFlags.None)
            });
            Options helpIdOptions = new(new TerminalUnicodeTextHandler(), new Option[] { new Option(helpIdOptionDescriptors.First().Value, true) });
            helpIdCommand = MockCommands.NewCommandDefinition("helpId1", "helpIdName", "helpIdDesc", CommandType.SubCommand, CommandFlags.None, helpIdOptionDescriptors, options: helpIdOptions);

            // This mocks the help alias request
            OptionDescriptors helpAliasOptionDescriptors = new(new TerminalUnicodeTextHandler(), new List<OptionDescriptor>()
            {
                new OptionDescriptor(terminalOptions.Help.OptionAlias, nameof(Boolean), "Help alias options", OptionFlags.None)
            });
            Options helpAliasOptions = new(new TerminalUnicodeTextHandler(), new Option[] { new Option(helpAliasOptionDescriptors.First().Value, true) });
            helpAliasCommand = MockCommands.NewCommandDefinition("helpAlias", "helpAliasName", "helpAliasDesc", CommandType.SubCommand, CommandFlags.None, helpAliasOptionDescriptors, options: helpAliasOptions);

            handler = new CommandHandler(host.Services, licenseChecker, terminalOptions, new LoggerFactory().CreateLogger<CommandHandler>());

            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            host?.Dispose();
            return Task.CompletedTask;
        }

        private Tuple<CommandDescriptor, Command> command = null!;
        private Tuple<CommandDescriptor, Command> helpIdCommand = null!;
        private Tuple<CommandDescriptor, Command> helpAliasCommand = null!;
        private CommandHandler handler = null!;
        private IHost host = null!;
        private License license = null!;
        private MockLicenseCheckerInner licenseChecker = null!;
        private TerminalOptions terminalOptions = null!;
        private CommandRouterContext routerContext = null!;
        private TerminalRouterContext routingContext = null!;
        private CancellationTokenSource terminalTokenSource = null!;
        private CancellationTokenSource commandTokenSource = null!;
    }
}