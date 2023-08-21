/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Commands.Extractors;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Mocks
{
    internal class MockCommandRouteParser : ICommandRouteParser
    {
        public Task<Root> ParseAsync(CommandRoute commandRoute)
        {
            return Task.FromResult(Root.Default());
        }
    }
}