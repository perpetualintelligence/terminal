/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using OneImlx.Terminal.Commands.Handlers.Mocks;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Commands.Runners.Mocks;
using OneImlx.Terminal.Mocks;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
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
            routingContext = new MockTerminalRouterContext(TerminalStartMode.Custom, commandTokenSource.Token);
            routerContext = new CommandContext(new(Guid.NewGuid().ToString(), "test"), routingContext, null);
        }

        [Fact]
        public async Task DelegateHelpShouldCallHelpAsync()
        {
            TerminalRequest request = new("id1", "test1");
            Command command = new(new CommandDescriptor("id", "name", "desc", CommandType.SubCommand, CommandFlags.None));
            ParsedCommand extractedCommand = new(command, null);
            routerContext.ParsedCommand = extractedCommand;
            routerContext.License = MockLicenses.TestLicense;

            MockTerminalHelpProvider helpProvider = new();
            MockDefaultCommandRunner mockCommandRunner = new();
            var result = await mockCommandRunner.DelegateHelpAsync(routerContext, helpProvider);
            mockCommandRunner.HelpCalled.Should().BeTrue();
            helpProvider.HelpCalled.Should().BeTrue();
            mockCommandRunner.RunCalled.Should().BeFalse();
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task DelegateRunShouldCallRunAsync()
        {
            TerminalRequest request = new("id1", "test1");
            Command command = new(new CommandDescriptor("id", "name", "desc", CommandType.SubCommand, CommandFlags.None));
            ParsedCommand extractedCommand = new(command, null);
            routerContext.ParsedCommand = extractedCommand;
            routerContext.License = MockLicenses.TestLicense;

            MockDefaultCommandRunner mockCommandRunner = new();
            var result = await mockCommandRunner.DelegateRunAsync(routerContext);
            mockCommandRunner.RunCalled.Should().BeTrue();
            mockCommandRunner.HelpCalled.Should().BeFalse();
            result.Should().BeOfType<MockCommandRunnerInnerResult>();
        }

        [Fact]
        public async Task HelpShouldThrowIfIHelpProviderIsNullAsync()
        {
            TerminalRequest request = new("id1", "test1");
            Command command = new(new CommandDescriptor("id", "name", "desc", CommandType.SubCommand, CommandFlags.None));
            ParsedCommand extractedCommand = new(command, null);
            routerContext.ParsedCommand = extractedCommand;
            routerContext.License = MockLicenses.TestLicense;

            MockDefaultCommandRunner mockCommandRunner = new();
            Func<Task> act = () => mockCommandRunner.RunHelpAsync(routerContext);
            await act.Should().ThrowAsync<TerminalException>().WithMessage("The help provider is missing in the configured services.");
        }

        private readonly CancellationTokenSource commandTokenSource = null!;
        private readonly CommandContext routerContext = null!;
        private readonly TerminalRouterContext routingContext = null!;
        private readonly CancellationTokenSource terminalTokenSource = null!;
    }
}
