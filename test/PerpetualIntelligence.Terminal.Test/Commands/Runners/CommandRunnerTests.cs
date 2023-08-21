/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Commands.Handlers.Mocks;
using PerpetualIntelligence.Terminal.Commands.Routers;
using PerpetualIntelligence.Terminal.Commands.Runners.Mocks;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Terminal.Runtime;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Terminal.Commands.Runners
{
    public class CommandRunnerTests
    {
        public CommandRunnerTests()
        {
            tokenSource = new CancellationTokenSource();
            routingContext = new MockTerminalRoutingContext(new TerminalStartContext(new TerminalStartInfo(TerminalStartMode.Custom), tokenSource.Token));
            routerContext = new CommandRouterContext("test", routingContext);
        }

        [Fact]
        public async Task HelpShouldThrowIfIHelpProviderIsNullAsync()
        {
            CommandRoute commandRoute = new("id1", "test1");
            Command command = new(new CommandDescriptor("id", "name", "prefix", "desc"));
            ExtractedCommand extractedCommand = new(commandRoute, command, Root.Default());

            CommandHandlerContext handlerContext = new(routerContext, extractedCommand, MockLicenses.TestLicense);
            MockDefaultCommandRunner mockCommandRunner = new();
            Func<Task> act = () => mockCommandRunner.HelpAsync(new CommandRunnerContext(handlerContext));
            await act.Should().ThrowAsync<ErrorException>().WithMessage("The help provider is missing in the configured services.");
        }

        [Fact]
        public async Task DelegateHelpShouldCallHelpAsync()
        {
            CommandRoute commandRoute = new("id1", "test1");
            Command command = new(new CommandDescriptor("id", "name", "prefix", "desc"));
            ExtractedCommand extractedCommand = new(commandRoute, command, Root.Default());

            CommandHandlerContext handlerContext = new(routerContext, extractedCommand, MockLicenses.TestLicense);
            MockHelpProvider helpProvider = new();
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
            CommandRoute commandRoute = new("id1", "test1");
            Command command = new(new CommandDescriptor("id", "name", "prefix", "desc"));
            ExtractedCommand extractedCommand = new(commandRoute, command, Root.Default());

            CommandHandlerContext handlerContext = new(routerContext, extractedCommand, MockLicenses.TestLicense);
            MockDefaultCommandRunner mockCommandRunner = new();
            var result = await mockCommandRunner.DelegateRunAsync(new CommandRunnerContext(handlerContext));
            mockCommandRunner.RunCalled.Should().BeTrue();
            mockCommandRunner.HelpCalled.Should().BeFalse();
            result.Should().BeOfType<MockCommandRunnerInnerResult>();
        }

        private CommandRouterContext routerContext = null!;
        private TerminalRoutingContext routingContext = null!;
        private CancellationTokenSource tokenSource = null!;
    }
}