/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Runtime;
using System.Collections.Generic;
using System.Threading;

namespace OneImlx.Terminal.Mocks
{
    internal class MockTerminalRouterContext : TerminalRouterContext
    {
        public MockTerminalRouterContext(
            TerminalStartMode startMode,
            CancellationToken commandCancellationToken,
            Dictionary<string, object>? customProperties = null,
            string[]? arguments = null) : base(startMode, commandCancellationToken, customProperties, arguments)
        {
        }
    }
}
