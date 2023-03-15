/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Cli.Commands.Runners;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Handlers.Mocks
{
    internal class MockGenericCommandRunnerInner : CommandRunner<MockGenericCommandRunnerResult>
    {
        public bool Called { get; private set; }

        public override Task<MockGenericCommandRunnerResult> RunAsync(CommandRunnerContext context)
        {
            Called = true;
            return Task.FromResult(new MockGenericCommandRunnerResult());
        }
    }
}