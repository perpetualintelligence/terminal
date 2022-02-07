/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PerpetualIntelligence.Cli.Commands.Publishers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Protocols.Abstractions;
using PerpetualIntelligence.Protocols.Cli;
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
        /// <param name="timeout">
        /// The routing timeout in milliseconds. The timeout applies to the
        /// <see cref="IRouter{TContext, TResult, THandler}.RouteAsync(TContext)"/> method.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="title">The command title to show in the console.</param>
        [WriteDocumentation("Add info about exception handling for ErrorException")]
        public static async Task RunRouterAsync(this IHost host, string title, int? timeout, CancellationToken? cancellationToken)
        {
            // Track the application lifetime so we can know whether cancellation is requested.
            IHostApplicationLifetime? applicationLifetime = host.Services.GetService<IHostApplicationLifetime>();

            while (true)
            {
                // Avoid block threads during cancellation and let the
                // applicationLifetime.ApplicationStopping.IsCancellationRequested get synchronized so we can honor the
                // app shutdown
                await Task.Delay(200);

                // Honor the cancellation request.
                if (cancellationToken.GetValueOrDefault().IsCancellationRequested)
                {
                    IErrorPublisher errorPublisher = host.Services.GetRequiredService<IErrorPublisher>();
                    ErrorPublisherContext errContext = new(new Shared.Infrastructure.Error(Errors.RequestCanceled, "Received cancellation token, the routing is canceled."));
                    await errorPublisher.PublishAsync(errContext);

                    // We are done, break the loop.
                    break;
                }

                // Check if application is stopping
                if (applicationLifetime != null && applicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    IErrorPublisher errorPublisher = host.Services.GetRequiredService<IErrorPublisher>();
                    ErrorPublisherContext errContext = new(new Shared.Infrastructure.Error(Errors.RequestCanceled, "Application is stopping, the routing is canceled."));
                    await errorPublisher.PublishAsync(errContext);

                    // We are done, break the loop.
                    break;
                }

                // Print the title
                Console.Write(title);

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
                    bool success = routeTask.Wait(timeout ?? Timeout.Infinite, cancellationToken ?? CancellationToken.None);
                    if (!success)
                    {
                        throw new TimeoutException($"The request timed out in {timeout} milliseconds.");
                    }

                    // This means a success in command runner. Wait for the next command
                }
                catch (Exception ex)
                {
                    // Task.Wait bundles up any exception into Exception.InnerException
                    IExceptionPublisher exceptionPublisher = host.Services.GetRequiredService<IExceptionPublisher>();
                    ExceptionPublisherContext exContext = new(raw, ex.InnerException ?? ex);
                    await exceptionPublisher.PublishAsync(exContext);
                }
            };
        }
    }
}
