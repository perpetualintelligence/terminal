/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Runtime;

namespace PerpetualIntelligence.Terminal.Mocks
{
    public class MockRoutingContext : TerminalRoutingContext
    {
        public MockRoutingContext(TerminalStartContext startContext) : base(startContext)
        {
        }
    }
}