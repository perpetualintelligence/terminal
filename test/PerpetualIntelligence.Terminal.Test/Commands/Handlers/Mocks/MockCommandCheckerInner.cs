/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Checkers;
using PerpetualIntelligence.Shared.Exceptions;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Handlers.Mocks
{
    internal class MockCommandCheckerInner : ICommandChecker
    {
        public bool ThrowException { get; set; }

        public bool Called { get; private set; }

        public Task<CommandCheckerResult> CheckAsync(CommandCheckerContext context)
        {
            Called = true;

            if (ThrowException)
            {
                throw new ErrorException("test_c_error", "test_c_desc");
            }

            return Task.FromResult(new CommandCheckerResult());
        }
    }
}