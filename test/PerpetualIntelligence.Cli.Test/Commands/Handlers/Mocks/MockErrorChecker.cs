/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Shared.Infrastructure;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Handlers.Mocks
{
    internal class MockErrorChecker : ICommandChecker
    {
        public Task<CommandCheckerResult> CheckAsync(CommandCheckerContext context)
        {
            return Task.FromResult(OneImlxResult.NewError<CommandCheckerResult>("test_checker_error", "test_checker_error_desc"));
        }
    }
}
