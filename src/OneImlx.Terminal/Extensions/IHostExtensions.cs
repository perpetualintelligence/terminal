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
        /// Runs the <see cref="ITerminalRouter{TContext}"/> asynchronously, blocking the calling thread until a cancellation request.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="context">The routing context for terminal console routing.</param>
        public static async Task RunTerminalRouterAsync<TRouting, TContext>(this IHost host, TContext context) where TRouting : class, ITerminalRouter<TContext> where TContext : TerminalRouterContext
        {
            ILogger<ITerminalRouter<TContext>> logger = host.Services.GetRequiredService<ILogger<ITerminalRouter<TContext>>>();

            logger.LogDebug("Start terminal router. type={0} context={1}", typeof(TRouting).Name, typeof(TContext).Name);
            ITerminalRouter<TContext> routingService = host.Services.GetRequiredService<ITerminalRouter<TContext>>();
            await routingService.RunAsync(context);
            logger.LogDebug("End terminal router.");
        }
    }
}