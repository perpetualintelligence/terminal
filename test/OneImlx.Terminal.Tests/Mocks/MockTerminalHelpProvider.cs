/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Mocks
{
    internal class MockTerminalHelpProvider : ITerminalHelpProvider
    {
        public bool HelpCalled { get; private set; }

        public Task ProvideHelpAsync(TerminalHelpProviderContext context)
        {
            HelpCalled = true;
            return Task.CompletedTask;
        }
    }
}