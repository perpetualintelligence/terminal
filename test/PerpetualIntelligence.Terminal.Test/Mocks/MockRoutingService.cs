/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Cli.Commands.Routers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    internal class MockRoutingService : IRoutingService
    {
        public bool Called
        {
            get; private set;
        }

        public Task RouteAsync(RoutingServiceContext context)
        {
            Called = true;
            return Task.CompletedTask;
        }
    }
}