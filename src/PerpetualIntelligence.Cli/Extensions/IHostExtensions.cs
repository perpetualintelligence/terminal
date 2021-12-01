/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands;
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
        /// Returns a task that routes command to its handler and blocks the calling thread till shutdown.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="title">The command title to show in the console.</param>
        public static async Task RunRoutingAsync(this IHost host, string? title = "cmd > ")
        {
            // FOMAC: check IHost.RunAsync to see how async is implemented
            while (true)
            {
                // FOMAC: avoid blocking threads.
                Thread.Sleep(200);

                // Read a command
                Console.Write(title);
                string? commandString = Console.ReadLine();

                // Ignore empty commands
                if (string.IsNullOrWhiteSpace(commandString))
                {
                    continue;
                }

                // Route the request.
                CommandContext context = new(commandString, host.Services);
                ICommandRouter router = host.Services.GetRequiredService<ICommandRouter>();
                CommandResult routed = await router.RouteAsync(context);
                if (routed.IsError)
                {
                    CliOptions options = host.Services.GetRequiredService<CliOptions>();
                    ILogger<CommandContext> logger = host.Services.GetRequiredService<ILogger<CommandContext>>();
                    logger.FormatAndLog(LogLevel.Error, options.Logging, "The request routing failed. path={0}", commandString);
                }
            };
        }
    }
}
