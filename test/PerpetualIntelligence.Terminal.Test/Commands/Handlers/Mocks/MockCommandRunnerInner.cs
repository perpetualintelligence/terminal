/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Providers;
using PerpetualIntelligence.Terminal.Commands.Runners;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Handlers.Mocks
{
    internal class MockCommandRunnerInner : IDelegateCommandRunner, ICommandRunner<CommandRunnerResult>
    {
        public bool DelegateRunCalled { get; set; }

        public bool HelpCalled { get; set; }
        public bool RunCalled { get; private set; }
        public bool DelegateHelpCalled { get; private set; }

        private IHelpProvider helpProvider = null!;

        public async Task<CommandRunnerResult> DelegateHelpAsync(CommandRunnerContext context, IHelpProvider helpProvider)
        {
            this.helpProvider = helpProvider;
            DelegateHelpCalled = true;
            await HelpAsync(context);
            return CommandRunnerResult.NoProcessing;
        }

        public Task<CommandRunnerResult> DelegateRunAsync(CommandRunnerContext context)
        {
            DelegateRunCalled = true;
            return RunAsync(context);
        }

        public async Task HelpAsync(CommandRunnerContext context)
        {
            await helpProvider.ProvideHelpAsync(new HelpProviderContext(context.Command));
            HelpCalled = true;
        }

        public Task<CommandRunnerResult> RunAsync(CommandRunnerContext context)
        {
            RunCalled = true;
            return Task.FromResult<CommandRunnerResult>(new MockCommandRunnerInnerResult());
        }
    }
}