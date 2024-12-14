/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Runners;

namespace OneImlx.Terminal.Mocks
{
    public class MockCommandRunner : ICommandRunner<CommandRunnerResult>
    {
        public bool HelpCalled { get; set; }

        public bool RunCalled { get; set; }

        public Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            RunCalled = true;
            return Task.FromResult(new CommandRunnerResult());
        }

        public Task RunHelpAsync(CommandContext context)
        {
            HelpCalled = true;
            return Task.FromResult(new CommandRunnerResult());
        }
    }
}
