/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Handlers.Mocks
{
    internal class MockCommandRunnerInner : IDelegateCommandRunner, ICommandRunner<CommandRunnerResult>
    {
        public bool DelegateHelpCalled { get; private set; }

        public bool DelegateRunCalled { get; set; }

        public bool HelpCalled { get; set; }

        public bool RunCalled { get; private set; }

        public async Task<CommandRunnerResult> DelegateHelpAsync(CommandRouterContext context, ITerminalHelpProvider helpProvider, ILogger? logger = null)
        {
            this.helpProvider = helpProvider;
            DelegateHelpCalled = true;
            await RunHelpAsync(context);
            return new CommandRunnerResult();
        }

        public Task<CommandRunnerResult> DelegateRunAsync(CommandRouterContext context, ILogger? logger = null)
        {
            DelegateRunCalled = true;
            return RunCommandAsync(context);
        }

        public Task<CommandRunnerResult> RunCommandAsync(CommandRouterContext context)
        {
            RunCalled = true;
            return Task.FromResult<CommandRunnerResult>(new MockCommandRunnerInnerResult());
        }

        public async Task RunHelpAsync(CommandRouterContext context)
        {
            await helpProvider.ProvideHelpAsync(new TerminalHelpProviderContext(context.EnsureParsedCommand().Command));
            HelpCalled = true;
        }

        private ITerminalHelpProvider helpProvider = null!;
    }
}
