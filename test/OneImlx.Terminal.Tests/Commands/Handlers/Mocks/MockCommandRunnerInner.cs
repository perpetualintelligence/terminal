/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Providers;
using OneImlx.Terminal.Commands.Runners;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Handlers.Mocks
{
    internal class MockCommandRunnerInner : IDelegateCommandRunner, ICommandRunner<CommandRunnerResult>
    {
        public bool DelegateRunCalled { get; set; }

        public bool HelpCalled { get; set; }
        public bool RunCalled { get; private set; }
        public bool DelegateHelpCalled { get; private set; }

        private IHelpProvider helpProvider = null!;

        public async Task<CommandRunnerResult> DelegateHelpAsync(CommandRunnerContext context, IHelpProvider helpProvider, ILogger? logger = null)
        {
            this.helpProvider = helpProvider;
            DelegateHelpCalled = true;
            await RunHelpAsync(context);
            return CommandRunnerResult.NoProcessing;
        }

        public Task<CommandRunnerResult> DelegateRunAsync(CommandRunnerContext context, ILogger? logger = null)
        {
            DelegateRunCalled = true;
            return RunCommandAsync(context);
        }

        public async Task RunHelpAsync(CommandRunnerContext context)
        {
            await helpProvider.ProvideHelpAsync(new HelpProviderContext(context.HandlerContext.ParsedCommand.Command));
            HelpCalled = true;
        }

        public Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            RunCalled = true;
            return Task.FromResult<CommandRunnerResult>(new MockCommandRunnerInnerResult());
        }
    }
}