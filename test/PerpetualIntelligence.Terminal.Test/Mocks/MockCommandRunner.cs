/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Runners;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Mocks
{
    public class MockCommandRunner : ICommandRunner<CommandRunnerResult>
    {
        public bool RunCalled { get; set; }

        public bool HelpCalled { get; set; }

        public Task HelpAsync(CommandRunnerContext context)
        {
            HelpCalled = true;
            return Task.FromResult(new CommandRunnerResult());
        }

        public Task<CommandRunnerResult> RunAsync(CommandRunnerContext context)
        {
            RunCalled = true;
            return Task.FromResult(new CommandRunnerResult());
        }
    }
}