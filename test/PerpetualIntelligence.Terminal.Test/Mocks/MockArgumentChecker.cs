/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Checkers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Mocks
{
    public class MockArgumentChecker : IOptionChecker
    {
        public bool Called { get; set; }

        public Task<OptionCheckerResult> CheckAsync(OptionCheckerContext context)
        {
            Called = true;
            return Task.FromResult(new OptionCheckerResult(typeof(string)));
        }
    }
}
