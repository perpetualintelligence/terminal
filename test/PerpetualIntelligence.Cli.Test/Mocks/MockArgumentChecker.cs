/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Checkers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockArgumentChecker : IArgumentChecker
    {
        public bool Called { get; set; }

        public Task<ArgumentCheckerResult> CheckAsync(ArgumentCheckerContext context)
        {
            Called = true;
            return Task.FromResult(new ArgumentCheckerResult(typeof(string)));
        }
    }
}
