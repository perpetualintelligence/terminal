/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands.Runners;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Handlers.Mocks
{
    internal class MockGenericCommandRunnerInner : CommandRunner<MockGenericCommandRunnerResult>
    {
        public bool Called { get; private set; }

        public bool ThrowException { get; set; }

        public override Task<MockGenericCommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
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