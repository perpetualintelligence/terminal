/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;

namespace OneImlx.Terminal.Commands.Handlers.Mocks
{
    internal class MockErrorCommandCheckerInner : ICommandChecker
    {
        public Task<CommandCheckerResult> CheckCommandAsync(CommandRouterContext context)
        {
            throw new TerminalException("test_checker_error", "test_checker_error_desc");
        }
    }
}
