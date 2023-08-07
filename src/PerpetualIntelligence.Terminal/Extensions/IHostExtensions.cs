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
        /// Runs the <see cref="TerminalConsoleRouting"/> asynchronously, blocking the calling thread until a cancellation request.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="context">The routing context for terminal console routing.</param>
        /// <returns>A task representing the asynchronous operation with a result of <see cref="TerminalConsoleRoutingResult"/>.</returns>
        public static Task<TerminalConsoleRoutingResult> RunConsoleRoutingAsync(this IHost host, TerminalConsoleRoutingContext context)
        {
            TerminalConsoleRouting routingService = host.Services.GetRequiredService<TerminalConsoleRouting>();
            return routingService.RunAsync(context);
        }

        /// <summary>
        /// Runs the <see cref="TerminalTcpRouting"/> asynchronously, blocking the calling thread until a cancellation request.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="context">The routing context for terminal TCP routing.</param>
        /// <returns>A task representing the asynchronous operation with a result of <see cref="TerminalTcpRoutingResult"/>.</returns>
        public static Task<TerminalTcpRoutingResult> RunTcpRoutingAsync(this IHost host, TerminalTcpRoutingContext context)
        {
            TerminalTcpRouting routingService = host.Services.GetRequiredService<TerminalTcpRouting>();
            return routingService.RunAsync(context);
        }

        /// <summary>
        /// Runs the <see cref="TerminalCustomRouting"/> asynchronously, blocking the calling thread until a cancellation token.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="context">The routing context for terminal custom routing.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ErrorException">Thrown when the requested start mode is not valid for custom routing.</exception>
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