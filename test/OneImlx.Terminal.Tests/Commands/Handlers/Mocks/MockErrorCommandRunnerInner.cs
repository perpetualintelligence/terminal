/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Handlers.Mocks
{
    internal class MockErrorCommandRunnerInner : IDelegateCommandRunner, ICommandRunner<CommandRunnerResult>
    {
        public async Task<CommandRunnerResult> DelegateHelpAsync(CommandRunnerContext context, ITerminalHelpProvider helpProvider, ILogger? logger = null)
        {
            await RunHelpAsync(context);
            return CommandRunnerResult.NoProcessing;
        }

        public Task<CommandRunnerResult> DelegateRunAsync(CommandRunnerContext context, ILogger? logger = null)
        {
            return RunCommandAsync(context);
        }

        public Task RunHelpAsync(CommandRunnerContext context)
        {
            throw new TerminalException("test_runner_help_error", "test_runner_help_error_desc");
        }

        public Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            throw new TerminalException("test_runner_error", "test_runner_error_desc");
        }
    }
}