/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Checkers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Mocks
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