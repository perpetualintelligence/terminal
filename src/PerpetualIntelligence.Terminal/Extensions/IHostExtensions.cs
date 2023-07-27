/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Terminal.Runtime;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Extensions
{
    /// <summary>
    /// The <see cref="IHost"/> extension methods.
    /// </summary>
    public static class IHostExtensions
    {
        /// <summary>
        /// Returns a task that runs the <see cref="ConsoleRouting"/> that blocks the calling thread till a cancellation request.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="context"></param>
        public static Task<ConsoleRoutingResult> RunConsoleRoutingAsync(this IHost host, ConsoleRoutingContext context)
        {
            //  Make sure we have supported start context
            if (context.StartContext.StartInformation.StartMode != TerminalStartMode.Console)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The requested start mode is not valid for console routing. start_mode={0}", context.StartContext.StartInformation.StartMode);
            }

            ConsoleRouting routingService = host.Services.GetRequiredService<ConsoleRouting>();
            return routingService.RunAsync(context);
        }

        /// <summary>
        /// Returns a task that runs the <see cref="TcpRouting"/> that blocks the calling thread till a cancellation request.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="context"></param>
        public static Task<TcpRoutingResult> RunTcpRoutingAsync(this IHost host, TcpRoutingContext context)
        {
            //  Make sure we have supported start context
            if (context.StartContext.StartInformation.StartMode != TerminalStartMode.Tcp)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The requested start mode is not valid for console routing. start_mode={0}", context.StartContext.StartInformation.StartMode);
            }

            TcpRouting routingService = host.Services.GetRequiredService<TcpRouting>();
            return routingService.RunAsync(context);
        }

        /// <summary>
        /// Returns a task that runs the <see cref="CustomRouting"/> that blocks the calling thread till a cancellation token.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Task RunCustomRoutingAsync(this IHost host, CustomRoutingContext context)
        {
            //  Make sure we have supported start context
            if (context.StartContext.StartInformation.StartMode != TerminalStartMode.Custom)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The requested start mode is not valid for console routing. start_mode={0}", context.StartContext.StartInformation.StartMode);
            }

            CustomRouting routingService = host.Services.GetRequiredService<CustomRouting>();
            return routingService.RunAsync(context);
        }
    }
}