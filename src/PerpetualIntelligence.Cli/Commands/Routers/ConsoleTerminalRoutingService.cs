/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Hosting;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Configuration.Options;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Routers
{
    /// <summary>
    /// The default <see cref="IRoutingService"/>.
    /// </summary>
    public class ConsoleTerminalRoutingService : IRoutingService
    {
        private readonly IHostApplicationLifetime applicationLifetime;
        private readonly ICommandRouter commandRouter;
        private readonly IExceptionHandler exceptionHandler;
        private readonly IErrorHandler errorHandler;
        private readonly CliOptions options;

        /// <summary>
        ///
        /// </summary>
        /// <param name="applicationLifetime"></param>
        /// <param name="commandRouter"></param>
        /// <param name="exceptionHandler"></param>
        /// <param name="errorHandler"></param>
        /// <param name="options"></param>
        public ConsoleTerminalRoutingService(IHostApplicationLifetime applicationLifetime, ICommandRouter commandRouter, IExceptionHandler exceptionHandler, IErrorHandler errorHandler, CliOptions options)
        {
            this.applicationLifetime = applicationLifetime;
            this.commandRouter = commandRouter;
            this.exceptionHandler = exceptionHandler;
            this.errorHandler = errorHandler;
            this.options = options;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task<RoutingServiceResult> RouteAsync(RoutingServiceContext context)
        {
            return Task.Run(async () =>
            {
                // Track the application lifetime so we can know whether cancellation is requested.
                while (true)
                {
                    // Avoid block threads during cancellation and let the
                    // applicationLifetime.ApplicationStopping.IsCancellationRequested get synchronized so we can honor the
                    // app shutdown
                    await Task.Delay(options.Router.SyncDelay.GetValueOrDefault());

                    // Honor the cancellation request.
                    if (context.CancellationToken.IsCancellationRequested)
                    {
                        ErrorHandlerContext errContext = new(new Shared.Infrastructure.Error(Errors.RequestCanceled, "Received cancellation token, the routing is canceled."));
                        await errorHandler.HandleAsync(errContext);

                        // We are done, break the loop.
                        break;
                    }

                    // Check if application is stopping
                    if (applicationLifetime.ApplicationStopping.IsCancellationRequested)
                    {
                        ErrorHandlerContext errContext = new(new Shared.Infrastructure.Error(Errors.RequestCanceled, $"Application is stopping, the routing is canceled."));
                        await errorHandler.HandleAsync(errContext);

                        // We are done, break the loop.
                        break;
                    }

                    // Print the caret
                    if (options.Router.Caret != null)
                    {
                        Console.Write(options.Router.Caret);
                    }

                    // Read the user input
                    string? raw = Console.ReadLine();

                    // Ignore empty commands
                    if (string.IsNullOrWhiteSpace(raw))
                    {
                        // Wait for next command.
                        continue;
                    }

                    try
                    {
                        // Route the request.
                        CommandRouterContext routerContext = new(raw, context.CancellationToken);
                        Task<CommandRouterResult> routeTask = commandRouter.RouteAsync(routerContext);

                        bool success = routeTask.Wait(options.Router.Timeout, context.CancellationToken);
                        if (!success)
                        {
                            throw new TimeoutException($"The command router timed out in {options.Router.Timeout} milliseconds.");
                        }

                        // This means a success in command runner. Wait for the next command
                    }
                    catch (Exception ex)
                    {
                        // Task.Wait bundles up any exception into Exception.InnerException
                        ExceptionHandlerContext exContext = new(raw, ex.InnerException ?? ex);
                        await exceptionHandler.HandleAsync(exContext);
                    }
                };

                return new RoutingServiceResult();
            });
        }
    }
}