/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Runners;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Handlers.Mocks
{
    internal class MockCommandRunnerInner : ICommandRunner
    {
        public Task<CommandRunnerResult> RunAsync(CommandRunnerContext context)
        {
            return Task.FromResult<CommandRunnerResult>(new MockCommandRunnerInnerResult());
        }
    }
}
