/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Hosting;
using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Extensions
{
    /// <summary>
    /// The <see cref="IHostBuilder"/> extension methods.
    /// </summary>
    public static class IHostBuilderExtensions
    {
        /// <summary>
        /// Starts the <see cref="IHost"/> and runs the <see cref="ITerminalRouter{TContext}"/> asynchronously, blocking the calling thread until a cancellation request.
        /// </summary>
        /// <param name="hostBuilder">The host builder.</param>
        /// <param name="context">The routing context for terminal console routing.</param>
        public static Task RunTerminalRouterAsync<TRouting, TContext>(this IHostBuilder hostBuilder, TContext context) where TRouting : class, ITerminalRouter<TContext> where TContext : TerminalRouterContext
        {
            // Ensure that the entire scope of IHost and router is disposed when the host is stopped.
            return Task.Run(async () =>
            {
                // Start the host.
                using (IHost host = hostBuilder.Start())
                {
                    // Run terminal router indefinitely till cancelled.
                    await host.RunTerminalRouterAsync<TRouting, TContext>(context);
                }
            });
        }

        /// <summary>
        /// Starts the <see cref="IHost"/> and runs the <see cref="ITerminalRouter{TContext}"/> synchronously, blocking the calling thread until a cancellation request.
        /// </summary>
        /// <param name="hostBuilder">The host builder.</param>
        /// <param name="context">The routing context for terminal console routing.</param>
        public static void RunTerminalRouter<TRouting, TContext>(this IHostBuilder hostBuilder, TContext context) where TRouting : class, ITerminalRouter<TContext> where TContext : TerminalRouterContext
        {
            // Synchronous
            Task.WaitAll(RunTerminalRouterAsync<TRouting, TContext>(hostBuilder, context));
        }
    }
}