/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Threading;

namespace PerpetualIntelligence.Cli.Commands.Routers
{
    /// <summary>
    /// The  <see cref="IRoutingService"/>  context.
    /// </summary>
    public class RoutingServiceContext
    {
        /// <summary>
        /// The cancellation token.
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public RoutingServiceContext(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
        }
    }
}