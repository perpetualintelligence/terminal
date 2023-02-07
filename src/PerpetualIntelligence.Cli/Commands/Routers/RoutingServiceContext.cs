/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Net;
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
        /// The network IP endpoint server will connect. Used for <see cref="TcpServerRoutingService"/>.
        /// </summary>
        public IPEndPoint? IPEndPoint { get; set; }

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