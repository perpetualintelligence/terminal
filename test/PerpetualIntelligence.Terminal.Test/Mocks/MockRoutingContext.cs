/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Runtime;
using System.Threading;

namespace PerpetualIntelligence.Terminal.Mocks
{
    public class MockRoutingContext : TerminalRoutingContext
    {
        public MockRoutingContext(TerminalStartContext startContext, CancellationToken cancellationToken) : base(startContext, cancellationToken)
        {
        }
    }
}