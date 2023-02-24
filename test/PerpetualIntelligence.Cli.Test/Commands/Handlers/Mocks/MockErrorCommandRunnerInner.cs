/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Runners;
using PerpetualIntelligence.Shared.Exceptions;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Handlers.Mocks
{
    internal class MockErrorCommandRunnerInner : IDelegateCommandRunner, ICommandRunner<CommandRunnerResult>
    {
        public Task<CommandRunnerResult> DelegateRunAsync(CommandRunnerContext context)
        {
            return RunAsync(context);
        }

        public Task<CommandRunnerResult> RunAsync(CommandRunnerContext context)
        {
            throw new ErrorException("test_runner_error", "test_runner_error_desc");
        }
    }
}