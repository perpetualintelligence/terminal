/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Shared;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Handlers.Mocks
{
    public class MockCommandCheckerInner : ICommandChecker
    {
        public bool Called { get; private set; }

        public bool ThrowException { get; set; }

        public Task<CommandCheckerResult> CheckCommandAsync(CommandContext context)
        {
            Called = true;

            if (ThrowException)
            {
                throw new TerminalException("test_c_error", "test_c_desc");
            }

            return Task.FromResult((CommandCheckerResult)new MockCommandCheckerInnerResult());
        }
    }
}
