/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Abstractions;
using PerpetualIntelligence.Shared.Extensions;
using System;
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
        /// Returns a task that runs the <see cref="ICommandRouter"/> and blocks the calling thread till a cancellation request.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="timeout">
        /// The routing timeout in milliseconds. The timeout applies to the
        /// <see cref="IRouter{TContext, TResult, THandler}.RouteAsync(TContext)"/> method.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="title">The command title to show in the console.</param>
        public static async Task RunRouterAsync(this IHost host, string title, int? timeout, CancellationToken? cancellationToken)
        {
            // Track the application lifetime so we can know whether cancellation is requested.
            IHostApplicationLifetime? applicationLifetime = host.Services.GetService<IHostApplicationLifetime>();

            // FOMAC: check IHost.RunAsync to see how async is implemented
            while (true)
            {
                // Avoid block threads during cancellation and let the
                // applicationLifetime.ApplicationStopping.IsCancellationRequested get synchronized so we can honor the
                // app shutdown
                await Task.Delay(200);

                // Honor the cancellation request.
                if (cancellationToken.GetValueOrDefault().IsCancellationRequested)
                {
                    CliOptions options = host.Services.GetRequiredService<CliOptions>();
                    ILogger<CommandRouterContext> logger = host.Services.GetRequiredService<ILogger<CommandRouterContext>>();
                    logger.FormatAndLog(LogLevel.Warning, options.Logging, "Received cancellation token, the routing is canceled.");

                    // We are done, break the loop.
                    break;
                }

                // Check if application is stopping
                if (applicationLifetime != null && applicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    CliOptions options = host.Services.GetRequiredService<CliOptions>();
                    ILogger<CommandRouterContext> logger = host.Services.GetRequiredService<ILogger<CommandRouterContext>>();
                    logger.FormatAndLog(LogLevel.Warning, options.Logging, "Application is stopping, the routing is canceled.");

                    // We are done, break the loop.
                    break;
                }

                // Print the title
                Console.Write(title);

                // Read the user input
                string? commandString = Console.ReadLine();

                // Ignore empty commands
                if (string.IsNullOrWhiteSpace(commandString))
                {
                    // Wait for next command.
                    continue;
                }

                // Route the request.
                CommandRouterContext context = new(commandString, cancellationToken);
                ICommandRouter router = host.Services.GetRequiredService<ICommandRouter>();
                Task routeTask = router.RouteAsync(context);

                try
                {
                    bool success = routeTask.Wait(timeout ?? Timeout.Infinite, cancellationToken ?? CancellationToken.None);
                    if (!success)
                    {
                        CliOptions options = host.Services.GetRequiredService<CliOptions>();
                        ILogger<CommandRouterContext> logger = host.Services.GetRequiredService<ILogger<CommandRouterContext>>();
                        logger.FormatAndLog(LogLevel.Error, options.Logging, "The request timed out. path={0}", commandString);
                    }
                }
                catch (OperationCanceledException)
                {
                    CliOptions options = host.Services.GetRequiredService<CliOptions>();
                    ILogger<CommandRouterContext> logger = host.Services.GetRequiredService<ILogger<CommandRouterContext>>();
                    logger.FormatAndLog(LogLevel.Error, options.Logging, "The request was canceled. path={0}", commandString);
                }
                catch (Exception ex)
                {
                    CliOptions options = host.Services.GetRequiredService<CliOptions>();
                    ILogger<CommandRouterContext> logger = host.Services.GetRequiredService<ILogger<CommandRouterContext>>();
                    logger.FormatAndLog(LogLevel.Error, options.Logging, "The request failed. path={0} additional_info={1}", commandString, ex.InnerException.Message);
                }
            };
        }

        private static void RouterCancellationDelegate()
        {
            throw new NotImplementedException();
        }
    }
}
