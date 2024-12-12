/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneImlx.Terminal.Commands.Handlers.Mocks;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using OneImlx.Test.FluentAssertions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Commands.Handlers
{
    public class CommandHandlerTests : IAsyncLifetime
    {
        [Fact]
        public async Task CallsCheckerEvent_If_TerminalEventHandler_Is_Configured()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            terminalEventHandler.BeforeCheckCalled.Should().Be(false);
            terminalEventHandler.AfterCheckCalled.Should().Be(false);

            TerminalRequest request = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(command.Item2, null);
            routerContext.License = license;
            routerContext.ParsedCommand = extractedCommand;

            await handler.HandleCommandAsync(routerContext);

            terminalEventHandler.BeforeCheckCalled.Should().Be(true);
            terminalEventHandler.AfterCheckCalled.Should().Be(true);
        }

        [Fact]
        public async Task CallsRunnerEvent_If_TerminalEventHandler_Is_Configured()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            terminalEventHandler.BeforeRunCalled.Should().Be(false);
            terminalEventHandler.AfterRunCalled.Should().Be(false);

            TerminalRequest request = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(command.Item2, null);
            routerContext.License = license;
            routerContext.ParsedCommand = extractedCommand;

            await handler.HandleCommandAsync(routerContext);

            terminalEventHandler.BeforeRunCalled.Should().Be(true);
            terminalEventHandler.AfterRunCalled.Should().Be(true);
        }

        [Fact]
        public async Task CheckerErrorShouldErrorHandler()
        {
            command.Item1.Checker = typeof(MockErrorCommandCheckerInner);
            commandRuntime.ReturnThisChecker = new MockErrorCommandCheckerInner();

            TerminalRequest request = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(command.Item2, null);
            routerContext.License = license;
            routerContext.ParsedCommand = extractedCommand;

            Func<Task> func = () => handler.HandleCommandAsync(routerContext);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode("test_checker_error").WithErrorDescription("test_checker_error_desc");
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task DoesNotCallAfterCheckerEvent_If_TerminalEventHandler_ConfiguredWithAnError()
        {
            command.Item1.Checker = typeof(MockErrorCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            commandRuntime.ReturnThisChecker = new MockErrorCommandCheckerInner();

            terminalEventHandler.BeforeCheckCalled.Should().BeFalse();
            terminalEventHandler.AfterCheckCalled.Should().BeFalse();

            try
            {
                TerminalRequest request = new("test_id", "test_raw");
                ParsedCommand extractedCommand = new(command.Item2, null);
                routerContext.License = license;
                routerContext.ParsedCommand = extractedCommand;

                await handler.HandleCommandAsync(routerContext);
            }
            catch (TerminalException eex)
            {
                eex.Error.ErrorCode.Should().Be("test_checker_error");
                eex.Error.ErrorDescription.Should().Be("test_checker_error_desc");
            }

            terminalEventHandler.BeforeCheckCalled.Should().BeTrue();
            terminalEventHandler.AfterCheckCalled.Should().BeFalse();

            terminalEventHandler.BeforeRunCalled.Should().BeFalse();
            terminalEventHandler.AfterRunCalled.Should().BeFalse();
        }

        [Fact]
        public async Task DoesNotCallAfterRunnerEvent_If_TerminalEventHandler_ConfiguredWithAnError()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockErrorCommandRunnerInner);

            commandRuntime.ReturnThisRunner = new MockErrorCommandRunnerInner();

            terminalEventHandler.BeforeRunCalled.Should().BeFalse();
            terminalEventHandler.AfterRunCalled.Should().BeFalse();

            try
            {
                TerminalRequest request = new("test_id", "test_raw");
                ParsedCommand extractedCommand = new(command.Item2, null);
                routerContext.License = license;
                routerContext.ParsedCommand = extractedCommand;

                await handler.HandleCommandAsync(routerContext);
            }
            catch (TerminalException eex)
            {
                eex.Error.ErrorCode.Should().Be("test_runner_error");
                eex.Error.ErrorDescription.Should().Be("test_runner_error_desc");
            }

            terminalEventHandler.BeforeRunCalled.Should().BeTrue();
            terminalEventHandler.AfterRunCalled.Should().BeFalse();
        }

        [Fact]
        public async Task DoesNotError_If_TerminalEventHandler_Is_NOT_Configured()
        {
            // Pass null to terminal event handler
            handler = new CommandHandler(commandRuntime, licenseChecker, terminalOptions, terminalHelpProvider, new LoggerFactory().CreateLogger<CommandHandler>());

            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            TerminalRequest request = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(command.Item2, null);
            routerContext.License = license;
            routerContext.ParsedCommand = extractedCommand;

            await handler.HandleCommandAsync(routerContext);

            routerContext.EnsureResult().EnsureCheckerResult().Should().BeOfType<MockCommandCheckerInnerResult>();
            routerContext.EnsureResult().EnsureRunnerResult().Should().BeOfType<MockCommandRunnerInnerResult>();
        }

        [Fact]
        public async Task DoesNotError_If_TerminalEventHandler_Is_NOT_Configured_Via_Constructor()
        {
            // Pass null to terminal event handler
            handler = new CommandHandler(commandRuntime, licenseChecker, terminalOptions, terminalHelpProvider, new LoggerFactory().CreateLogger<CommandHandler>());

            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            ParsedCommand extractedCommand = new(command.Item2, null);
            routerContext.License = license;
            routerContext.ParsedCommand = extractedCommand;

            await handler.HandleCommandAsync(routerContext);

            routerContext.EnsureResult().EnsureCheckerResult().Should().BeOfType<MockCommandCheckerInnerResult>();
            routerContext.EnsureResult().EnsureRunnerResult().Should().BeOfType<MockCommandRunnerInnerResult>();
        }

        [Fact]
        public async Task HandleCommand_Returns_ExpectedChecker_Results()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            ParsedCommand extractedCommand = new(command.Item2, null);
            routerContext.License = license;
            routerContext.ParsedCommand = extractedCommand;

            routerContext.Result.Should().BeNull();
            await handler.HandleCommandAsync(routerContext);

            CommandRouterResult? result = routerContext.Result;
            result.Should().NotBeNull();

            result!.CheckerResult.Should().NotBeNull();
            result.CheckerResult.Should().BeOfType<MockCommandCheckerInnerResult>();

            result.RunnerResult.Should().NotBeNull();
            result.RunnerResult.Should().BeOfType<MockCommandRunnerInnerResult>();
        }

        [Fact]
        public async Task HandleCommand_Returns_ExpectedGenericsRunner_Results()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockGenericCommandRunnerInner);

            ParsedCommand extractedCommand = new(command.Item2, null);
            routerContext.License = license;
            routerContext.ParsedCommand = extractedCommand;

            commandRuntime.ReturnThisRunner = new MockGenericCommandRunnerInner();

            routerContext.Result.Should().BeNull();
            await handler.HandleCommandAsync(routerContext);

            CommandRouterResult? result = routerContext.Result;
            result.Should().NotBeNull();

            result!.RunnerResult.Should().NotBeNull();
            result.RunnerResult.Should().BeOfType<MockGenericCommandRunnerResult>();
        }

        [Fact]
        public async Task HandleCommand_Returns_ExpectedRunner_Results()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            ParsedCommand extractedCommand = new(command.Item2, null);
            routerContext.License = license;
            routerContext.ParsedCommand = extractedCommand;

            routerContext.Result.Should().BeNull();
            await handler.HandleCommandAsync(routerContext);

            CommandRouterResult? result = routerContext.Result;
            result.Should().NotBeNull();

            result!.RunnerResult.Should().NotBeNull();
            result.RunnerResult.Should().BeOfType<MockCommandRunnerInnerResult>();
        }

        [Fact]
        public async Task HelpErrorShouldErrorHandler()
        {
            // Make sure checker pass so runner can fail
            helpIdCommand.Item1.Checker = typeof(MockCommandCheckerInner);
            helpIdCommand.Item1.Runner = typeof(MockErrorCommandRunnerInner);

            commandRuntime.ReturnThisRunner = new MockErrorCommandRunnerInner();

            ParsedCommand extractedCommand = new(helpIdCommand.Item2, null);
            routerContext.License = license;
            routerContext.ParsedCommand = extractedCommand;

            Func<Task> func = () => handler.HandleCommandAsync(routerContext);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode("test_runner_help_error").WithErrorDescription("test_runner_help_error_desc");
        }

        [Fact]
        public async Task HelpShouldBeCalledAndReturnsResult()
        {
            helpIdCommand.Item1.Checker = typeof(MockCommandCheckerInner);
            helpIdCommand.Item1.Runner = typeof(MockCommandRunnerInner);

            ParsedCommand extractedCommand = new(helpIdCommand.Item2, null);
            routerContext.License = license;
            routerContext.ParsedCommand = extractedCommand;

            routerContext.Result.Should().BeNull();
            await handler.HandleCommandAsync(routerContext);

            routerContext.EnsureResult().RunnerResult.Should().NotBeNull();
        }

        [Fact]
        public async Task HelpShouldBeCalledIfEnabledAndRequested()
        {
            helpIdCommand.Item1.Checker = typeof(MockCommandCheckerInner);
            helpIdCommand.Item1.Runner = typeof(MockCommandRunnerInner);

            ParsedCommand extractedCommand = new(helpIdCommand.Item2, null);
            routerContext.License = license;
            routerContext.ParsedCommand = extractedCommand;

            MockCommandRunnerInner runner = new();
            commandRuntime.ReturnThisRunner = runner;

            await handler.HandleCommandAsync(routerContext);

            // Resolve checker not called for Help
            commandRuntime.ResolveCheckerCalled.Should().BeFalse();
            commandRuntime.ResolveRunnerCalled.Should().BeTrue();

            // Help called
            terminalHelpProvider.HelpCalled.Should().BeTrue();

            // runner calls delegate help
            runner.DelegateHelpCalled.Should().BeTrue();
            runner.HelpCalled.Should().BeTrue();

            // runner does not call delegate run
            runner.DelegateRunCalled.Should().BeFalse();
            runner.RunCalled.Should().BeFalse();
        }

        [Fact]
        public async Task HelpShouldNotBeCalledIfDisabledAndRequested()
        {
            terminalOptions.Value.Help.Disabled = true;

            helpIdCommand.Item1.Checker = typeof(MockCommandCheckerInner);
            helpIdCommand.Item1.Runner = typeof(MockCommandRunnerInner);

            ParsedCommand extractedCommand = new(helpIdCommand.Item2, null);
            routerContext.License = license;
            routerContext.ParsedCommand = extractedCommand;

            await handler.HandleCommandAsync(routerContext);

            terminalHelpProvider.HelpCalled.Should().BeFalse();
        }

        [Fact]
        public async Task HelpShouldNotBeCalledIfEnabledAndNotRequested()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            ParsedCommand extractedCommand = new(command.Item2, null);
            routerContext.License = license;
            routerContext.ParsedCommand = extractedCommand;

            await handler.HandleCommandAsync(routerContext);

            terminalHelpProvider.HelpCalled.Should().BeFalse();
        }

        public Task InitializeAsync()
        {
            terminalTokenSource = new CancellationTokenSource();
            commandTokenSource = new CancellationTokenSource();
            terminalOptions = Microsoft.Extensions.Options.Options.Create(MockTerminalOptions.NewLegacyOptions());
            license = MockLicenses.TestLicense;
            licenseChecker = new MockLicenseCheckerInner();
            command = MockCommands.NewCommandDefinition("id1", "name1", "desc1", CommandType.SubCommand, CommandFlags.None);
            routingContext = new MockTerminalRouterContext(new TerminalStartContext(TerminalStartMode.Custom, terminalTokenSource.Token, commandTokenSource.Token));
            routerContext = new CommandRouterContext(new(Guid.NewGuid().ToString(), "test"), routingContext, null);
            commandRuntime = new MockCommandResolver();
            terminalHelpProvider = new MockTerminalHelpProvider();
            terminalEventHandler = new MockTerminalEventHandler();

            // This mocks the help id request
            OptionDescriptors helpIdOptionDescriptors = new(new TerminalUnicodeTextHandler(),
            [
                new(terminalOptions.Value.Help.OptionId, nameof(Boolean), "Help options", OptionFlags.None)
            ]);
            Options helpIdOptions = new(new TerminalUnicodeTextHandler(), [new(helpIdOptionDescriptors.First().Value, true)]);
            helpIdCommand = MockCommands.NewCommandDefinition("helpId1", "helpIdName", "helpIdDesc", CommandType.SubCommand, CommandFlags.None, helpIdOptionDescriptors, options: helpIdOptions);

            // This mocks the help alias request
            OptionDescriptors helpAliasOptionDescriptors = new(new TerminalUnicodeTextHandler(),
            [
                new(terminalOptions.Value.Help.OptionAlias, nameof(Boolean), "Help alias options", OptionFlags.None)
            ]);
            Options helpAliasOptions = new(new TerminalUnicodeTextHandler(), [new(helpAliasOptionDescriptors.First().Value, true)]);
            helpAliasCommand = MockCommands.NewCommandDefinition("helpAlias", "helpAliasName", "helpAliasDesc", CommandType.SubCommand, CommandFlags.None, helpAliasOptionDescriptors, options: helpAliasOptions);

            handler = new CommandHandler(commandRuntime, licenseChecker, terminalOptions, terminalHelpProvider, new LoggerFactory().CreateLogger<CommandHandler>(), terminalEventHandler);

            return Task.CompletedTask;
        }

        [Fact]
        public async Task RunHelpAliasShouldSkipCommandChecker()
        {
            helpAliasCommand.Item1.Checker = typeof(MockCommandCheckerInner);
            helpAliasCommand.Item1.Runner = typeof(MockCommandRunnerInner);

            ParsedCommand extractedCommand = new(helpAliasCommand.Item2, null);
            routerContext.License = license;
            routerContext.ParsedCommand = extractedCommand;

            MockCommandCheckerInner checker = new();
            commandRuntime.ReturnThisChecker = checker;

            await handler.HandleCommandAsync(routerContext);

            terminalHelpProvider.HelpCalled.Should().BeTrue();
            checker.Called.Should().BeFalse();
        }

        [Fact]
        public async Task RunHelpIdShouldSkipCommandChecker()
        {
            helpIdCommand.Item1.Checker = typeof(MockCommandCheckerInner);
            helpIdCommand.Item1.Runner = typeof(MockCommandRunnerInner);

            ParsedCommand extractedCommand = new(helpIdCommand.Item2, null);
            routerContext.License = license;
            routerContext.ParsedCommand = extractedCommand;

            MockCommandCheckerInner checker = new();
            commandRuntime.ReturnThisChecker = checker;

            await handler.HandleCommandAsync(routerContext);

            terminalHelpProvider.HelpCalled.Should().BeTrue();
            checker.Called.Should().BeFalse();
        }

        [Fact]
        public async Task RunnerErrorShouldErrorHandler()
        {
            // Make sure checker pass so runner can fail
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockErrorCommandRunnerInner);

            ParsedCommand extractedCommand = new(command.Item2, null);
            routerContext.License = license;
            routerContext.ParsedCommand = extractedCommand;

            commandRuntime.ReturnThisRunner = new MockErrorCommandRunnerInner();

            Func<Task> func = () => handler.HandleCommandAsync(routerContext);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode("test_runner_error").WithErrorDescription("test_runner_error_desc");
        }

        [Fact]
        public async Task RunShouldBeCalled()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            ParsedCommand extractedCommand = new(command.Item2, null);
            routerContext.License = license;
            routerContext.ParsedCommand = extractedCommand;

            await handler.HandleCommandAsync(routerContext);

            commandRuntime.ResolveCheckerCalled.Should().BeTrue();
            commandRuntime.ResolveRunnerCalled.Should().BeTrue();

            MockCommandRunnerInner? commandRunnerInner = (MockCommandRunnerInner?)commandRuntime.ReturnedRunner;
            commandRunnerInner.Should().NotBeNull();
            commandRunnerInner!.DelegateHelpCalled.Should().BeFalse();
            commandRunnerInner.HelpCalled.Should().BeFalse();

            commandRunnerInner.DelegateRunCalled.Should().BeTrue();
            commandRunnerInner.RunCalled.Should().BeTrue();
        }

        [Fact]
        public async Task RunShouldBeCalledIfHelpIsDisabled()
        {
            terminalOptions.Value.Help.Disabled = true;

            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            ParsedCommand extractedCommand = new(command.Item2, null);
            routerContext.License = license;
            routerContext.ParsedCommand = extractedCommand;

            await handler.HandleCommandAsync(routerContext);

            commandRuntime.ResolveCheckerCalled.Should().BeTrue();
            commandRuntime.ResolveRunnerCalled.Should().BeTrue();

            MockCommandRunnerInner? commandRunnerInner = (MockCommandRunnerInner?)commandRuntime.ReturnedRunner;
            commandRunnerInner.Should().NotBeNull();
            commandRunnerInner!.DelegateHelpCalled.Should().BeFalse();
            commandRunnerInner.HelpCalled.Should().BeFalse();

            commandRunnerInner.DelegateRunCalled.Should().BeTrue();
            commandRunnerInner.RunCalled.Should().BeTrue();
        }

        [Fact]
        public async Task ValidCheckerAndRunnerShouldCheckCorrectLicenseFeaturesAsync()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            ParsedCommand extractedCommand = new(command.Item2, null);
            routerContext.License = license;
            routerContext.ParsedCommand = extractedCommand;

            await handler.HandleCommandAsync(routerContext);

            licenseChecker.Called.Should().BeTrue();
            licenseChecker.PassedLicense.Should().NotBeNull();
            licenseChecker.PassedLicense!.Should().BeSameAs(license);
        }

        private Tuple<CommandDescriptor, Command> command = null!;
        private MockCommandResolver commandRuntime = null!;
        private CancellationTokenSource commandTokenSource = null!;
        private CommandHandler handler = null!;
        private Tuple<CommandDescriptor, Command> helpAliasCommand = null!;
        private Tuple<CommandDescriptor, Command> helpIdCommand = null!;
        private License license = null!;
        private MockLicenseCheckerInner licenseChecker = null!;
        private CommandRouterContext routerContext = null!;
        private TerminalRouterContext routingContext = null!;
        private MockTerminalEventHandler terminalEventHandler = null!;
        private MockTerminalHelpProvider terminalHelpProvider = null!;
        private IOptions<TerminalOptions> terminalOptions = null!;
        private CancellationTokenSource terminalTokenSource = null!;
    }
}
