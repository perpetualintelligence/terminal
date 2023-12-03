/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Mocks
{
    public class MockRoutingContext : TerminalRouterContext
    {
        public MockRoutingContext(TerminalStartContext startContext) : base(startContext)
        {
        }
    }
}