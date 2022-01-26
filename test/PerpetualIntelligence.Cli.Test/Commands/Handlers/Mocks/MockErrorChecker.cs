/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Shared.Exceptions;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Handlers.Mocks
{
    internal class MockErrorChecker : ICommandChecker
    {
        public Task<CommandCheckerResult> CheckAsync(CommandCheckerContext context)
        {
            throw new ErrorException("test_checker_error", "test_checker_error_desc");
        }
    }
}
