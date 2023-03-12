/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Handlers.Mocks;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Runners.Mocks
{
    internal class MockDefaultCommandRunner : CommandRunner<CommandRunnerResult>
    {
        public bool HelpCalled { get; private set; }
        public bool RunCalled { get; private set; }

        public override async Task HelpAsync(CommandRunnerContext context)
        {
            HelpCalled = true;
            await base.HelpAsync(context);
        }

        public override Task<CommandRunnerResult> RunAsync(CommandRunnerContext context)
        {
            RunCalled = true;
            return Task.FromResult((CommandRunnerResult)new MockCommandRunnerInnerResult());
        }
    }
}