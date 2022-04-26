/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Shared.Attributes;
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="caret">The command caret to show in the console.</param>
        [WriteDocumentation("Add info about exception handling for ErrorException")]
        public static async Task RunRouterAsync(this IHost host, string? caret = null, CancellationToken? cancellationToken = default)
        {
            // Track the application lifetime so we can know whether cancellation is requested.
            IHostApplicationLifetime applicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
            CliOptions cliOptions = host.Services.GetRequiredService<CliOptions>();

            while (true)
            {
                // Avoid block threads during cancellation and let the
                // applicationLifetime.ApplicationStopping.IsCancellationRequested get synchronized so we can honor the
                // app shutdown
                await Task.Delay(200);

                // Honor the cancellation request.
                if (cancellationToken.GetValueOrDefault().IsCancellationRequested)
                {
                    IErrorHandler errorPublisher = host.Services.GetRequiredService<IErrorHandler>();
                    ErrorHandlerContext errContext = new(new Shared.Infrastructure.Error(Errors.RequestCanceled, "Received cancellation token, the routing is canceled."));
                    await errorPublisher.PublishAsync(errContext);

                    // We are done, break the loop.
                    break;
                }

                // Check if application is stopping
                if (applicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    IErrorHandler errorPublisher = host.Services.GetRequiredService<IErrorHandler>();
                    ErrorHandlerContext errContext = new(new Shared.Infrastructure.Error(Errors.RequestCanceled, $"Application is stopping, the routing is canceled."));
                    await errorPublisher.PublishAsync(errContext);

                    // We are done, break the loop.
                    break;
                }

                // Print the caret
                if (caret != null)
                {
                    Console.Write(caret);
                }

                // Read the user input
                string? raw = Console.ReadLine();

                // Ignore empty commands
                if (string.IsNullOrWhiteSpace(raw))
                {
                    // Wait for next command.
                    continue;
                }

                // Route the request.
                CommandRouterContext context = new(raw, cancellationToken);
                ICommandRouter router = host.Services.GetRequiredService<ICommandRouter>();
                Task<CommandRouterResult> routeTask = router.RouteAsync(context);

                try
                {
                    bool success = routeTask.Wait(cliOptions.Router.Timeout, cancellationToken ?? CancellationToken.None);
                    if (!success)
                    {
                        throw new TimeoutException($"The command router timed out in {cliOptions.Router.Timeout} milliseconds.");
                    }

                    // This means a success in command runner. Wait for the next command
                }
                catch (Exception ex)
                {
                    // Task.Wait bundles up any exception into Exception.InnerException
                    IExceptionHandler exceptionPublisher = host.Services.GetRequiredService<IExceptionHandler>();
                    ExceptionHandlerContext exContext = new(raw, ex.InnerException ?? ex);
                    await exceptionPublisher.PublishAsync(exContext);
                }
            };
        }
    }
}
