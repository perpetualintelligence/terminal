/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Commands.Handlers.Mocks;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Commands.Runners.Mocks;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OneImlx.Terminal.Commands.Runners
{
    public class CommandRunnerTests
    {
        public CommandRunnerTests()
        {
            terminalTokenSource = new CancellationTokenSource();
            commandTokenSource = new CancellationTokenSource();
            routingContext = new MockTerminalRouterContext(new TerminalStartContext(TerminalStartMode.Custom, terminalTokenSource.Token, commandTokenSource.Token));
            routerContext = new CommandRouterContext("test", routingContext, null);
        }

        [Fact]
        public async Task HelpShouldThrowIfIHelpProviderIsNullAsync()
        {
            TerminalProcessorRequest request = new("id1", "test1");
            Command command = new(new CommandDescriptor("id", "name", "desc", CommandType.SubCommand, CommandFlags.None));
            ParsedCommand extractedCommand = new(request, command, Root.Default());

            CommandHandlerContext handlerContext = new(routerContext, extractedCommand, MockLicenses.TestLicense);
            MockDefaultCommandRunner mockCommandRunner = new();
            Func<Task> act = () => mockCommandRunner.RunHelpAsync(new CommandRunnerContext(handlerContext));
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The help provider is missing in the configured services.");
        }

        [Fact]
        public async Task DelegateHelpShouldCallHelpAsync()
        {
            TerminalProcessorRequest request = new("id1", "test1");
            Command command = new(new CommandDescriptor("id", "name", "desc", CommandType.SubCommand, CommandFlags.None));
            ParsedCommand extractedCommand = new(request, command, Root.Default());

            CommandHandlerContext handlerContext = new(routerContext, extractedCommand, MockLicenses.TestLicense);
            MockTerminalHelpProvider helpProvider = new();
            MockDefaultCommandRunner mockCommandRunner = new();
            var result = await mockCommandRunner.DelegateHelpAsync(new CommandRunnerContext(handlerContext), helpProvider);
            mockCommandRunner.HelpCalled.Should().BeTrue();
            helpProvider.HelpCalled.Should().BeTrue();
            mockCommandRunner.RunCalled.Should().BeFalse();
            result.Should().BeEquivalentTo(CommandRunnerResult.NoProcessing);
        }

        [Fact]
        public async Task DelegateRunShouldCallRunAsync()
        {
            TerminalProcessorRequest request = new("id1", "test1");
            Command command = new(new CommandDescriptor("id", "name", "desc", CommandType.SubCommand, CommandFlags.None));
            ParsedCommand extractedCommand = new(request, command, Root.Default());

            CommandHandlerContext handlerContext = new(routerContext, extractedCommand, MockLicenses.TestLicense);
            MockDefaultCommandRunner mockCommandRunner = new();
            var result = await mockCommandRunner.DelegateRunAsync(new CommandRunnerContext(handlerContext));
            mockCommandRunner.RunCalled.Should().BeTrue();
            mockCommandRunner.HelpCalled.Should().BeFalse();
            result.Should().BeOfType<MockCommandRunnerInnerResult>();
        }

        private readonly CommandRouterContext routerContext = null!;
        private readonly TerminalRouterContext routingContext = null!;
        private readonly CancellationTokenSource terminalTokenSource = null!;
        private readonly CancellationTokenSource commandTokenSource = null!;
    }
}