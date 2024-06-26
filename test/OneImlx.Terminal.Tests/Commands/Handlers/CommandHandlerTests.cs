﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Handlers.Mocks;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Configuration.Options;
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
        public async Task CheckerErrorShouldErrorHandler()
        {
            command.Item1.Checker = typeof(MockErrorCommandCheckerInner);
            commandRuntime.ReturnThisChecker = new MockErrorCommandCheckerInner();

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            Func<Task> func = () => handler.HandleCommandAsync(commandContext);
            await func.Should().ThrowAsync<TerminalException>().WithErrorCode("test_checker_error").WithErrorDescription("test_checker_error_desc");
        }

        [Fact]
        public async Task Handler_Does_Process_And_Dispose_ResultAsync()
        {
            MockCommandRunnerInnerResult tempResult = new();
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
        public async Task RunnerErrorShouldErrorHandler()
        {
            // Make sure checker pass so runner can fail
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockErrorCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            commandRuntime.ReturnThisRunner = new MockErrorCommandRunnerInner();

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

            commandRuntime.ReturnThisRunner = new MockErrorCommandRunnerInner();

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

            MockCommandCheckerInner checker = new();
            commandRuntime.ReturnThisChecker = checker;

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await handler.HandleCommandAsync(commandContext);

            terminalHelpProvider.HelpCalled.Should().BeTrue();
            checker.Called.Should().BeFalse();
        }

        [Fact]
        public async Task RunHelpAliasShouldSkipCommandChecker()
        {
            helpAliasCommand.Item1.Checker = typeof(MockCommandCheckerInner);
            helpAliasCommand.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, helpAliasCommand.Item2, Root.Default());

            MockCommandCheckerInner checker = new();
            commandRuntime.ReturnThisChecker = checker;

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await handler.HandleCommandAsync(commandContext);

            terminalHelpProvider.HelpCalled.Should().BeTrue();
            checker.Called.Should().BeFalse();
        }

        [Fact]
        public async Task HelpShouldBeCalledIfEnabledAndRequested()
        {
            helpIdCommand.Item1.Checker = typeof(MockCommandCheckerInner);
            helpIdCommand.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, helpIdCommand.Item2, Root.Default());

            MockCommandRunnerInner runner = new();
            commandRuntime.ReturnThisRunner = runner;

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await handler.HandleCommandAsync(commandContext);

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
        public async Task RunShouldBeCalled()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await handler.HandleCommandAsync(commandContext);

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
        public async Task HelpShouldNotBeCalledIfDisabledAndRequested()
        {
            terminalOptions.Help.Disabled = true;

            helpIdCommand.Item1.Checker = typeof(MockCommandCheckerInner);
            helpIdCommand.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, helpIdCommand.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            await handler.HandleCommandAsync(commandContext);

            terminalHelpProvider.HelpCalled.Should().BeFalse();
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

            terminalHelpProvider.HelpCalled.Should().BeFalse();
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
        public async Task HandleCommand_Returns_ExpectedRunner_Results()
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
        public async Task HandleCommand_Returns_ExpectedGenericsRunner_Results()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockGenericCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            commandRuntime.ReturnThisRunner = new MockGenericCommandRunnerInner();

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            var result = await handler.HandleCommandAsync(commandContext);
            result.Should().NotBeNull();
            result.Should().BeOfType<CommandHandlerResult>();

            result.RunnerResult.Should().NotBeNull();
            result.RunnerResult.Should().BeOfType<MockGenericCommandRunnerResult>();
        }

        [Fact]
        public async Task HandleCommand_Returns_ExpectedChecker_Results()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            var result = await handler.HandleCommandAsync(commandContext);
            result.Should().NotBeNull();
            result.Should().BeOfType<CommandHandlerResult>();

            result.CheckerResult.Should().NotBeNull();
            result.CheckerResult.Should().BeOfType<MockCommandCheckerInnerResult>();

            result.RunnerResult.Should().NotBeNull();
            result.RunnerResult.Should().BeOfType<MockCommandRunnerInnerResult>();
        }

        [Fact]
        public async Task DoesNotError_If_TerminalEventHandler_Is_NOT_Configured()
        {
            // Pass null to terminal event handler
            handler = new CommandHandler(commandRuntime, licenseChecker, terminalOptions, terminalHelpProvider, null, new LoggerFactory().CreateLogger<CommandHandler>());

            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            var result = await handler.HandleCommandAsync(commandContext);

            result.CheckerResult.Should().BeOfType<MockCommandCheckerInnerResult>();
            result.RunnerResult.Should().BeOfType<MockCommandRunnerInnerResult>();
        }

        [Fact]
        public async Task DoesNotError_If_TerminalEventHandler_Is_NOT_Configured_Via_Constructor()
        {
            // Pass null to terminal event handler
            handler = new CommandHandler(commandRuntime, licenseChecker, terminalOptions, terminalHelpProvider, new LoggerFactory().CreateLogger<CommandHandler>());

            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            var result = await handler.HandleCommandAsync(commandContext);

            result.CheckerResult.Should().BeOfType<MockCommandCheckerInnerResult>();
            result.RunnerResult.Should().BeOfType<MockCommandRunnerInnerResult>();
        }

        [Fact]
        public async Task CallsCheckerEvent_If_TerminalEventHandler_Is_Configured()
        {
            command.Item1.Checker = typeof(MockCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            terminalEventHandler.BeforeCheckCalled.Should().Be(false);
            terminalEventHandler.AfterCheckCalled.Should().Be(false);

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            var result = await handler.HandleCommandAsync(commandContext);

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

            CommandRoute commandRoute = new("test_id", "test_raw");
            ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

            CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
            var result = await handler.HandleCommandAsync(commandContext);

            terminalEventHandler.BeforeRunCalled.Should().Be(true);
            terminalEventHandler.AfterRunCalled.Should().Be(true);
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
                CommandRoute commandRoute = new("test_id", "test_raw");
                ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

                CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
                var result = await handler.HandleCommandAsync(commandContext);
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
        public async Task DoesNotCallAfterCheckerEvent_If_TerminalEventHandler_ConfiguredWithAnError()
        {
            command.Item1.Checker = typeof(MockErrorCommandCheckerInner);
            command.Item1.Runner = typeof(MockCommandRunnerInner);

            commandRuntime.ReturnThisChecker = new MockErrorCommandCheckerInner();

            terminalEventHandler.BeforeCheckCalled.Should().BeFalse();
            terminalEventHandler.AfterCheckCalled.Should().BeFalse();

            try
            {
                CommandRoute commandRoute = new("test_id", "test_raw");
                ParsedCommand extractedCommand = new(commandRoute, command.Item2, Root.Default());

                CommandHandlerContext commandContext = new(routerContext, extractedCommand, license);
                var result = await handler.HandleCommandAsync(commandContext);
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

        public Task InitializeAsync()
        {
            terminalTokenSource = new CancellationTokenSource();
            commandTokenSource = new CancellationTokenSource();
            terminalOptions = MockTerminalOptions.NewLegacyOptions();
            license = MockLicenses.TestLicense;
            licenseChecker = new MockLicenseCheckerInner();
            command = MockCommands.NewCommandDefinition("id1", "name1", "desc1", CommandType.SubCommand, CommandFlags.None);
            routingContext = new MockTerminalRouterContext(new TerminalStartContext(TerminalStartMode.Custom, terminalTokenSource.Token, commandTokenSource.Token));
            routerContext = new CommandRouterContext("test", routingContext, null);
            commandRuntime = new MockCommandRuntime();
            terminalHelpProvider = new MockTerminalHelpProvider();
            terminalEventHandler = new MockTerminalEventHandler();

            // This mocks the help id request
            OptionDescriptors helpIdOptionDescriptors = new(new TerminalUnicodeTextHandler(), new List<OptionDescriptor>()
            {
                new(terminalOptions.Help.OptionId, nameof(Boolean), "Help options", OptionFlags.None)
            });
            Options helpIdOptions = new(new TerminalUnicodeTextHandler(), new Option[] { new(helpIdOptionDescriptors.First().Value, true) });
            helpIdCommand = MockCommands.NewCommandDefinition("helpId1", "helpIdName", "helpIdDesc", CommandType.SubCommand, CommandFlags.None, helpIdOptionDescriptors, options: helpIdOptions);

            // This mocks the help alias request
            OptionDescriptors helpAliasOptionDescriptors = new(new TerminalUnicodeTextHandler(), new List<OptionDescriptor>()
            {
                new(terminalOptions.Help.OptionAlias, nameof(Boolean), "Help alias options", OptionFlags.None)
            });
            Options helpAliasOptions = new(new TerminalUnicodeTextHandler(), new Option[] { new(helpAliasOptionDescriptors.First().Value, true) });
            helpAliasCommand = MockCommands.NewCommandDefinition("helpAlias", "helpAliasName", "helpAliasDesc", CommandType.SubCommand, CommandFlags.None, helpAliasOptionDescriptors, options: helpAliasOptions);

            handler = new CommandHandler(commandRuntime, licenseChecker, terminalOptions, terminalHelpProvider, terminalEventHandler, new LoggerFactory().CreateLogger<CommandHandler>());

            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        private Tuple<CommandDescriptor, Command> command = null!;
        private Tuple<CommandDescriptor, Command> helpIdCommand = null!;
        private Tuple<CommandDescriptor, Command> helpAliasCommand = null!;
        private CommandHandler handler = null!;
        private License license = null!;
        private MockLicenseCheckerInner licenseChecker = null!;
        private MockCommandRuntime commandRuntime = null!;
        private TerminalOptions terminalOptions = null!;
        private CommandRouterContext routerContext = null!;
        private TerminalRouterContext routingContext = null!;
        private CancellationTokenSource terminalTokenSource = null!;
        private CancellationTokenSource commandTokenSource = null!;
        private MockTerminalHelpProvider terminalHelpProvider = null!;
        private MockTerminalEventHandler terminalEventHandler = null!;
    }
}