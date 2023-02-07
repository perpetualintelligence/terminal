/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Routers
{
    /// <summary>
    /// The default <see cref="IRoutingService"/> for custom routing.
    /// </summary>
    public abstract class CustomRoutingService : IRoutingService
    {
        /// <summary>
        /// Routes to a custom service implementation.
        /// </summary>
        /// <param name="context">The routing service context.</param>
        /// <returns>The routing service result.</returns>
        public abstract Task<RoutingServiceResult> RouteAsync(RoutingServiceContext context);
    }
}