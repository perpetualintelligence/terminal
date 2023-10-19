/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        /// Runs the <see cref="ITerminalRouting{TContext, TResult}"/> asynchronously, blocking the calling thread until a cancellation request.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="context">The routing context for terminal console routing.</param>
        /// <returns>A task representing the asynchronous operation with a result of <see cref="TerminalConsoleRoutingResult"/>.</returns>
        public static async Task<TResult> RunTerminalRoutingAsync<TRouting, TContext, TResult>(this IHost host, TContext context) where TRouting : class, ITerminalRouting<TContext, TResult> where TContext : TerminalRoutingContext where TResult : TerminalRoutingResult
        {
            ILogger<ITerminalRouting<TContext, TResult>> logger = host.Services.GetRequiredService<ILogger<ITerminalRouting<TContext, TResult>>>();
            logger.LogDebug("Start terminal routing. routing={0} context={1} result={2}", typeof(TRouting).Name, typeof(TContext).Name, typeof(TResult).Name);

            ITerminalRouting<TContext, TResult> routingService = host.Services.GetRequiredService<ITerminalRouting<TContext, TResult>>();
            TResult result = await routingService.RunAsync(context);

            logger.LogDebug("End terminal routing.");
            return result;
        }
    }
}