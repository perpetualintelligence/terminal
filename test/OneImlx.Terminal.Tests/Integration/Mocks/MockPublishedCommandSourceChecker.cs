/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Integration;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminals.Integration.Mocks
{
    internal class MockPublishedCommandSourceChecker : ITerminalCommandSourceChecker<PublishedCommandSourceContext>
    {
        public bool Called { get; private set; }

        public PublishedCommandSourceContext PassedContext { get; private set; } = null!;

        public Task CheckSourceAsync(PublishedCommandSourceContext context)
        {
            Called = true;
            PassedContext = context;
            return Task.CompletedTask;
        }
    }
}