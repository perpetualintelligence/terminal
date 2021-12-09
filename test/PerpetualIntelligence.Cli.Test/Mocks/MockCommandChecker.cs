/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Checkers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockCommandChecker : ICommandChecker
    {
        public bool Called { get; set; }

        public Task<CommandCheckerResult> CheckAsync(CommandCheckerContext context)
        {
            Called = true;
            return Task.FromResult(new CommandCheckerResult());
        }
    }
}
