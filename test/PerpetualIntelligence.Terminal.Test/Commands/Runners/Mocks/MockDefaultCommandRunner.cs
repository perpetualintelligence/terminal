/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Handlers.Mocks;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Runners.Mocks
{
    internal class MockDefaultCommandRunner : CommandRunner<CommandRunnerResult>
    {
        public bool HelpCalled { get; private set; }
        public bool RunCalled { get; private set; }

        public override async Task RunHelpAsync(CommandRunnerContext context)
        {
            HelpCalled = true;
            await base.RunHelpAsync(context);
        }

        public override Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            RunCalled = true;
            return Task.FromResult((CommandRunnerResult)new MockCommandRunnerInnerResult());
        }
    }
}