/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Runners;

namespace OneImlx.Terminal.Commands.Handlers.Mocks
{
    internal class MockGenericCommandRunnerInner : CommandRunner<MockGenericCommandRunnerResult>
    {
        public bool Called { get; private set; }

        public bool ThrowException { get; set; }

        public override Task<MockGenericCommandRunnerResult> RunCommandAsync(CommandRouterContext context)
        {
            Called = true;

            if (ThrowException)
            {
                throw new TerminalException("test_error", "test_desc");
            }

            return Task.FromResult(new MockGenericCommandRunnerResult());
        }
    }
}
