/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Checkers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Handlers.Mocks
{
    internal class MockCommandCheckerInner : ICommandChecker
    {
        public bool ThrowException { get; set; }

        public bool Called { get; private set; }

        public Task<CommandCheckerResult> CheckCommandAsync(CommandCheckerContext context)
        {
            Called = true;

            if (ThrowException)
            {
                throw new TerminalException("test_c_error", "test_c_desc");
            }

            return Task.FromResult(new CommandCheckerResult());
        }
    }
}