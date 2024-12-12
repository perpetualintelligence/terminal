/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;

namespace OneImlx.Terminal.Mocks
{
    public class MockArgumentChecker : IArgumentChecker
    {
        public bool Called { get; set; }

        public Task<ArgumentCheckerResult> CheckArgumentAsync(Argument argument)
        {
            Called = true;
            return Task.FromResult(new ArgumentCheckerResult(typeof(string)));
        }
    }
}
