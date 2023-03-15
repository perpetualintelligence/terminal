/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Cli.Commands.Providers;
using PerpetualIntelligence.Cli.Commands.Runners;
using PerpetualIntelligence.Shared.Exceptions;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Handlers.Mocks
{
    internal class MockErrorCommandRunnerInner : IDelegateCommandRunner, ICommandRunner<CommandRunnerResult>
    {
        public async Task<CommandRunnerResult> DelegateHelpAsync(CommandRunnerContext context, IHelpProvider helpProvider)
        {
            await HelpAsync(context);
            return CommandRunnerResult.NoProcessing;
        }

        public Task<CommandRunnerResult> DelegateRunAsync(CommandRunnerContext context)
        {
            return RunAsync(context);
        }

        public Task HelpAsync(CommandRunnerContext context)
        {
            throw new ErrorException("test_runner_help_error", "test_runner_help_error_desc");
        }

        public Task<CommandRunnerResult> RunAsync(CommandRunnerContext context)
        {
            throw new ErrorException("test_runner_error", "test_runner_error_desc");
        }
    }
}