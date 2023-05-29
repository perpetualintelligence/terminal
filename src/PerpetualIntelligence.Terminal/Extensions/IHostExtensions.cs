/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Shared.Exceptions;
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
        /// <param name="context"></param>
        public static Task RunRouterAsTerminalAsync(this IHost host, RoutingServiceContext context)
        {
            IRoutingService routingService = host.Services.GetRequiredService<IRoutingService>();
            if (routingService is ConsoleRoutingService consoleTerminalRouting)
            {
                return consoleTerminalRouting.RouteAsync(context);
            }

            throw new ErrorException(Errors.InvalidConfiguration, "The configured routing service is not a console routing service.");
        }

        /// <summary>
        /// Returns a task that runs the <see cref="ICommandRouter"/> that listens for connections from TCP network clients and blocks the calling thread till a cancellation token.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Task RunRouterAsTcpServerAsync(this IHost host, RoutingServiceContext context)
        {
            IRoutingService routingService = host.Services.GetRequiredService<IRoutingService>();
            if (routingService is TcpRoutingService tcpServerRoutingService)
            {
                return tcpServerRoutingService.RouteAsync(context);
            }

            throw new ErrorException(Errors.InvalidConfiguration, "The configured routing service is not a TCP server routing service.");
        }

        /// <summary>
        /// Returns a task that runs the <see cref="ICommandRouter"/> that starts a custom service and blocks the calling thread till a cancellation token.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Task RunRouterAsCustomServiceAsync(this IHost host, RoutingServiceContext context)
        {
            IRoutingService routingService = host.Services.GetRequiredService<IRoutingService>();
            if (routingService is CustomRoutingService customRoutingService)
            {
                return customRoutingService.RouteAsync(context);
            }

            throw new ErrorException(Errors.InvalidConfiguration, "The configured routing service is not a custom routing service.");
        }
    }
}