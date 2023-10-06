/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        public static Task<TResult> RunTerminalRoutingAsync<TRouting, TContext, TResult>(this IHost host, TContext context) where TRouting : class, ITerminalRouting<TContext, TResult> where TContext : TerminalRoutingContext where TResult : TerminalRoutingResult
        {
            // TODO add checks for context, result and routing
            ITerminalRouting<TContext, TResult> routingService = host.Services.GetRequiredService<ITerminalRouting<TContext, TResult>>();
            return routingService.RunAsync(context);
        }
    }
}