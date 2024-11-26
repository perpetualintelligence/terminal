/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Mocks
{
    internal class MockTerminalRouter : ITerminalRouter<MockRoutingContext>
    {
        public bool Called
        {
            get; private set;
        }

        public bool IsRunning { get; private set; } = false;

        public Task RunAsync(MockRoutingContext context)
        {
            Called = true;
            IsRunning = true;
            return Task.CompletedTask;
        }
    }
}
