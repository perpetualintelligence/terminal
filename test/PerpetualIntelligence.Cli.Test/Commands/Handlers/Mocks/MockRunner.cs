/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Runners;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Handlers.Mocks
{
    internal class MockRunner : ICommandRunner
    {
        public Task<CommandRunnerResult> RunAsync(CommandRunnerContext context)
        {
            return Task.FromResult(new CommandRunnerResult());
        }
    }
}
