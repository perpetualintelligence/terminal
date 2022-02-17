/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Checkers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Handlers.Mocks
{
    internal class MockCommandCheckerInner : ICommandChecker
    {
        public Task<CommandCheckerResult> CheckAsync(CommandCheckerContext context)
        {
            // all ok
            return Task.FromResult(new CommandCheckerResult()
            {
            });
        }
    }
}
