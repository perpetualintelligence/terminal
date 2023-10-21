/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Commands.Routers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalRouting{TContext}"/> for console based terminals.
    /// </summary>
    public class TerminalConsoleRouting : ITerminalRouting<TerminalConsoleRoutingContext>
    {
        private readonly ITerminalConsole terminalConsole;
        private readonly IHostApplicationLifetime applicationLifetime;
        private readonly ICommandRouter commandRouter;
        private readonly IExceptionHandler exceptionHandler;
        private readonly TerminalOptions options;
        private readonly ILogger<TerminalConsoleRouting> logger;

        /// <summary>
        /// Initialize a new <see cref="TerminalConsoleRouting"/> instance.
        /// </summary>
        /// <param name="terminalConsole">The terminal console.</param>
        /// <param name="applicationLifetime">The host application lifetime instance.</param>
        /// <param name="commandRouter">The command router.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public TerminalConsoleRouting(
            ITerminalConsole terminalConsole,
            IHostApplicationLifetime applicationLifetime,
            ICommandRouter commandRouter,
            IExceptionHandler exceptionHandler,
            TerminalOptions options,
            ILogger<TerminalConsoleRouting> logger)
        {
            this.terminalConsole = terminalConsole;
            this.applicationLifetime = applicationLifetime;
            this.commandRouter = commandRouter;
            this.exceptionHandler = exceptionHandler;
            this.options = options;
            this.logger = logger;
        }

        /// <summary>
        /// Routes to the console asynchronously.
        /// </summary>
        /// <param name="context">The routing service context.</param>
        /// <returns></returns>
        public virtual Task RunAsync(TerminalConsoleRoutingContext context)
        {
            return Task.Run(async () =>
            {
                //  Make sure we have supported start context
                if (context.StartContext.StartInformation.StartMode != TerminalStartMode.Console)
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The requested start mode is not valid for console routing. start_mode={0}", context.StartContext.StartInformation.StartMode);
                }

                // Track the application lifetime so we can know whether cancellation is requested.
                while (true)
                {
                    CommandRoute? route = null;

                    try
                    {
                        // Wait for a bit to avoid CPU hogging and give time for cancellation token to be set.
                        await Task.Delay(100);

                        // Honor the cancellation request.
                        if (context.StartContext.CancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException("Received cancellation token, the routing is canceled.");
                        }

                        // Check if application is stopping
                        if (applicationLifetime.ApplicationStopping.IsCancellationRequested)
                        {
                            throw new OperationCanceledException("Application is stopping, the routing is canceled.");
                        }

                        // Print the caret
                        if (options.Router.Caret != null)
                        {
                            await terminalConsole.WriteAsync(options.Router.Caret);
                        }

                        // Read the user input
                        string? raw = await terminalConsole.ReadLineAsync();

                        // Ignore empty commands
                        if (raw == null || terminalConsole.Ignore(raw))
                        {
                            // Wait for next command.
                            logger.LogDebug("The raw string is null or ignored by the terminal console.");
                            continue;
                        }

                        // Route the request.
                        CommandRouterContext routerContext = new(raw, context);
                        route = routerContext.Route;
                        Task<CommandRouterResult> routeTask = commandRouter.RouteCommandAsync(routerContext);

                        bool success = routeTask.Wait(options.Router.Timeout, context.StartContext.CancellationToken);
                        if (!success)
                        {
                            throw new TimeoutException($"The command router timed out in {options.Router.Timeout} milliseconds.");
                        }
                    }
                    catch (OperationCanceledException oex)
                    {
                        // Routing is canceled.
                        ExceptionHandlerContext exContext = new(oex, route);
                        await exceptionHandler.HandleExceptionAsync(exContext);
                        break;
                    }
                    catch (Exception ex)
                    {
                        // Task.Wait bundles up any exception into Exception.InnerException
                        ExceptionHandlerContext exContext = new(ex.InnerException ?? ex, route);
                        await exceptionHandler.HandleExceptionAsync(exContext);
                    }
                };
            });
        }
    }
}