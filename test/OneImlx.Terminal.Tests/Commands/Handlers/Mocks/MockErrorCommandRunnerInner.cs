/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Commands.Handlers.Mocks
{
    internal class MockErrorCommandRunnerInner : IDelegateCommandRunner, ICommandRunner<CommandRunnerResult>
    {
        public async Task<CommandRunnerResult> DelegateHelpAsync(CommandRouterContext context, ITerminalHelpProvider helpProvider, ILogger? logger = null)
        {
            await RunHelpAsync(context);
            return new CommandRunnerResult();
        }

        public Task<CommandRunnerResult> DelegateRunAsync(CommandRouterContext context, ILogger? logger = null)
        {
            return RunCommandAsync(context);
        }

        public Task<CommandRunnerResult> RunCommandAsync(CommandRouterContext context)
        {
            throw new TerminalException("test_runner_error", "test_runner_error_desc");
        }

        public Task RunHelpAsync(CommandRouterContext context)
        {
            throw new TerminalException("test_runner_help_error", "test_runner_help_error_desc");
        }
    }
}
