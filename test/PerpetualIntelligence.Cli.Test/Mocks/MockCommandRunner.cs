/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Runners;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockCommandRunner : ICommandRunner<CommandRunnerContext, CommandRunnerResult>
    {
        public bool Called { get; set; }

        public Task<CommandRunnerResult> RunAsync(CommandRunnerContext context)
        {
            Called = true;
            return Task.FromResult(new CommandRunnerResult());
        }
    }
}