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
        /// Returns a task that runs the <see cref="TerminalConsoleRouting"/> that blocks the calling thread till a cancellation request.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="context"></param>
        public static Task<TerminalConsoleRoutingResult> RunConsoleRoutingAsync(this IHost host, TerminalConsoleRoutingContext context)
        {
            //  Make sure we have supported start context
            if (context.StartContext.StartInformation.StartMode != TerminalStartMode.Console)
            {
                throw new ErrorException(TerminalErrors.InvalidConfiguration, "The requested start mode is not valid for console routing. start_mode={0}", context.StartContext.StartInformation.StartMode);
            }

            TerminalConsoleRouting routingService = host.Services.GetRequiredService<TerminalConsoleRouting>();
            return routingService.RunAsync(context);
        }

        /// <summary>
        /// Returns a task that runs the <see cref="TerminalTcpRouting"/> that blocks the calling thread till a cancellation request.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="context"></param>
        public static Task<TerminalTcpRoutingResult> RunTcpRoutingAsync(this IHost host, TerminalTcpRoutingContext context)
        {
            //  Make sure we have supported start context
            if (context.StartContext.StartInformation.StartMode != TerminalStartMode.Tcp)
            {
                throw new ErrorException(TerminalErrors.InvalidConfiguration, "The requested start mode is not valid for console routing. start_mode={0}", context.StartContext.StartInformation.StartMode);
            }

            TerminalTcpRouting routingService = host.Services.GetRequiredService<TerminalTcpRouting>();
            return routingService.RunAsync(context);
        }

        /// <summary>
        /// Returns a task that runs the <see cref="TerminalCustomRouting"/> that blocks the calling thread till a cancellation token.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Task RunCustomRoutingAsync(this IHost host, TerminalCustomRoutingContext context)
        {
            //  Make sure we have supported start context
            if (context.StartContext.StartInformation.StartMode != TerminalStartMode.Custom)
            {
                throw new ErrorException(TerminalErrors.InvalidConfiguration, "The requested start mode is not valid for console routing. start_mode={0}", context.StartContext.StartInformation.StartMode);
            }

            TerminalCustomRouting routingService = host.Services.GetRequiredService<TerminalCustomRouting>();
            return routingService.RunAsync(context);
        }
    }
}