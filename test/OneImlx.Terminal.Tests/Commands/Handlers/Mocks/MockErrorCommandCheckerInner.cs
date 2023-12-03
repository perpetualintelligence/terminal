/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands.Checkers;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Handlers.Mocks
{
    internal class MockErrorCommandCheckerInner : ICommandChecker
    {
        public Task<CommandCheckerResult> CheckCommandAsync(CommandCheckerContext context)
        {
            throw new TerminalException("test_checker_error", "test_checker_error_desc");
        }
    }
}