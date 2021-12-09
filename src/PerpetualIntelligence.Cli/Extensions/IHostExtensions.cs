/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Cli.Configuration.Options;
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
        /// Returns a task that runs the command router and blocks the calling thread till shutdown.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="timeout">The routing timeout in milliseconds.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="title">The command title to show in the console.</param>
        public static async Task RunRoutingAsync(this IHost host, string? title, int timeout, CancellationToken cancellationToken)
        {
            // FOMAC: check IHost.RunAsync to see how async is implemented
            while (true)
            {
                // FOMAC: avoid blocking threads.
                await Task.Delay(200);

                // Read a command
                if (!string.IsNullOrWhiteSpace(title))
                {
                    Console.Write(title);
                }
                string? commandString = Console.ReadLine();

                // Ignore empty commands
                if (string.IsNullOrWhiteSpace(commandString))
                {
                    continue;
                }

                // Route the request.
                CommandRouterContext context = new(commandString);
                ICommandRouter router = host.Services.GetRequiredService<ICommandRouter>();
                Task routeTask = router.RouteAsync(context);

                try
                {
                    bool success = routeTask.Wait(timeout, cancellationToken);
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
                    logger.FormatAndLog(LogLevel.Error, options.Logging, "The request failed. path={0} additional_info={1}", commandString, ex.Message);
                }
            };
        }
    }
}
