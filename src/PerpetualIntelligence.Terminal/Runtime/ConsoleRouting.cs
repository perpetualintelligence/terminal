/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/
/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Commands.Routers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalRouting{TContext, TResult}"/> for console based terminals.
    /// </summary>
    public class ConsoleRouting : ITerminalRouting<ConsoleRoutingContext, ConsoleRoutingResult>
    {
        private readonly IHostApplicationLifetime applicationLifetime;
        private readonly ICommandRouter commandRouter;
        private readonly IExceptionHandler exceptionHandler;
        private readonly IErrorHandler errorHandler;
        private readonly TerminalOptions options;
        private readonly ILogger<ConsoleRouting> logger;

        /// <summary>
        /// Initialize a new <see cref="ConsoleRouting"/> instance.
        /// </summary>
        /// <param name="applicationLifetime">The host application lifetime instance.</param>
        /// <param name="commandRouter">The command router.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <param name="errorHandler">The error handler.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public ConsoleRouting(IHostApplicationLifetime applicationLifetime, ICommandRouter commandRouter, IExceptionHandler exceptionHandler, IErrorHandler errorHandler, TerminalOptions options, ILogger<ConsoleRouting> logger)
        {
            this.applicationLifetime = applicationLifetime;
            this.commandRouter = commandRouter;
            this.exceptionHandler = exceptionHandler;
            this.errorHandler = errorHandler;
            this.options = options;
            this.logger = logger;
        }

        /// <summary>
        /// Routes to the console asynchronously.
        /// </summary>
        /// <param name="context">The routing service context.</param>
        /// <returns></returns>
        public Task<ConsoleRoutingResult> RunAsync(ConsoleRoutingContext context)
        {
            return Task.Run(async () =>
            {
                // Track the application lifetime so we can know whether cancellation is requested.
                while (true)
                {
                    // Avoid blocking threads during cancellation and let the
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

                        // This means a success in command runner. Wait for the next command.
                        // Dispose the runner result as it is not propagated any further.
                        await routeTask.Result.HandlerResult.RunnerResult.DisposeAsync();
                    }
                    catch (Exception ex)
                    {
                        // Task.Wait bundles up any exception into Exception.InnerException
                        ExceptionHandlerContext exContext = new(raw, ex.InnerException ?? ex);
                        await exceptionHandler.HandleAsync(exContext);
                    }
                };

                // Return Result
                return new ConsoleRoutingResult();
            });
        }
    }
}