/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Runners;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockCommandRunner : ICommandRunner
    {
        public bool Called { get; set; }

        public Task<CommandRunnerResult> RunAsync(CommandRunnerContext context)
        {
            Called = true;
            return Task.FromResult(new CommandRunnerResult());
        }
    }
}
