/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using PerpetualIntelligence.Terminal.Commands.Handlers.Mocks;
using PerpetualIntelligence.Terminal.Commands.Routers;
using PerpetualIntelligence.Terminal.Commands.Runners.Mocks;
using PerpetualIntelligence.Terminal.Mocks;
using PerpetualIntelligence.Shared.Exceptions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Terminal.Commands.Runners
{
    public class CommandRunnerTests
    {
        [Fact]
        public async Task HelpShouldThrowIfIHelpProviderIsNullAsync()
        {
            MockDefaultCommandRunner mockCommandRunner = new();
            Func<Task> act = () => mockCommandRunner.HelpAsync(new CommandRunnerContext(new Command(new CommandRoute("id1", "test1"), new CommandDescriptor("id", "name", "prefix", "desc"))));
            await act.Should().ThrowAsync<ErrorException>().WithMessage("The help provider is missing in the configured services.");
        }

        [Fact]
        public async Task DelegateHelpShouldCallHelpAsync()
        {
            MockHelpProvider helpProvider = new MockHelpProvider();
            MockDefaultCommandRunner mockCommandRunner = new();
            var result = await mockCommandRunner.DelegateHelpAsync(new CommandRunnerContext(new Command(new CommandRoute("id1", "test1"), new CommandDescriptor("id", "name", "prefix", "desc"))), helpProvider);
            mockCommandRunner.HelpCalled.Should().BeTrue();
            helpProvider.HelpCalled.Should().BeTrue();
            mockCommandRunner.RunCalled.Should().BeFalse();
            result.Should().BeEquivalentTo(CommandRunnerResult.NoProcessing);
        }

        [Fact]
        public async Task DelegateRunShouldCallRunAsync()
        {
            MockDefaultCommandRunner mockCommandRunner = new();
            var result = await mockCommandRunner.DelegateRunAsync(new CommandRunnerContext(new Command(new CommandRoute("id1", "test1"), new CommandDescriptor("id", "name", "prefix", "desc"))));
            mockCommandRunner.RunCalled.Should().BeTrue();
            mockCommandRunner.HelpCalled.Should().BeFalse();
            result.Should().BeOfType<MockCommandRunnerInnerResult>();
        }
    }
}