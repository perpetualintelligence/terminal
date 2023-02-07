/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Shared.Attributes;
using PerpetualIntelligence.Shared.Exceptions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Extensions
{
    /// <summary>
    /// The <see cref="IHost"/> extension methods.
    /// </summary>
    public static class IHostExtensions
    {
        /// <summary>
        /// Returns a task that runs the <see cref="ICommandRouter"/> as a terminal console that blocks the calling thread till a cancellation request.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        [WriteDocumentation("Add info about exception handling for ErrorException")]
        public static Task<RoutingServiceResult> RunRouterAsTerminalAsync(this IHost host, CancellationToken cancellationToken)
        {
            IRoutingService routingService = host.Services.GetRequiredService<IRoutingService>();
            if (routingService is ConsoleRoutingService consoleTerminalRouting)
            {
                RoutingServiceContext context = new(cancellationToken);
                return consoleTerminalRouting.RouteAsync(context);
            }

            throw new ErrorException(Errors.InvalidConfiguration, "The configured routing service is not a console routing service.");
        }

        /// <summary>
        /// Returns a task that runs the <see cref="ICommandRouter"/> that listens for connections from TCP network clients and blocks the calling thread till a cancellation token.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="iPEndPoint">The network endpoint as an IP address and a port number.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task RunRouterAsTcpServerAsync(this IHost host, IPEndPoint iPEndPoint, CancellationToken cancellationToken)
        {
            IRoutingService routingService = host.Services.GetRequiredService<IRoutingService>();
            if (routingService is TcpServerRoutingService tcpServerRoutingService)
            {
                RoutingServiceContext context = new(cancellationToken)
                {
                    IPEndPoint = iPEndPoint,
                };
                return tcpServerRoutingService.RouteAsync(context);
            }

            throw new ErrorException(Errors.InvalidConfiguration, "The configured routing service is not a TCP server routing service.");
        }

        /// <summary>
        /// Returns a task that runs the <see cref="ICommandRouter"/> that starts a custom service and blocks the calling thread till a cancellation token.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task RunRouterAsCustomServiceAsync(this IHost host, CancellationToken cancellationToken)
        {
            IRoutingService routingService = host.Services.GetRequiredService<IRoutingService>();
            if (routingService is CustomRoutingService customRoutingService)
            {
                RoutingServiceContext context = new(cancellationToken);
                return customRoutingService.RouteAsync(context);
            }

            throw new ErrorException(Errors.InvalidConfiguration, "The configured routing service is not a custom routing service.");
        }
    }
}