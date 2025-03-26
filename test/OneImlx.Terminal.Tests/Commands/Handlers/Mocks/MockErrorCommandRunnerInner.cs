/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;

namespace OneImlx.Terminal.Commands.Handlers.Mocks
{
    internal class MockErrorCommandRunnerInner : IDelegateCommandRunner, ICommandRunner<CommandRunnerResult>
    {
        public async Task<CommandRunnerResult> DelegateHelpAsync(CommandContext context, ITerminalHelpProvider helpProvider, ILogger? logger = null)
        {
            await RunHelpAsync(context);
            return new CommandRunnerResult();
        }

        public Task<CommandRunnerResult> DelegateRunAsync(CommandContext context, ILogger? logger = null)
        {
            return RunCommandAsync(context);
        }

        public Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            throw new TerminalException("test_runner_error", "test_runner_error_desc");
        }

        public Task RunHelpAsync(CommandContext context)
        {
            throw new TerminalException("test_runner_help_error", "test_runner_help_error_desc");
        }
    }
}
