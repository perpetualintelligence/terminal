/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalRouter{TContext}"/> for console based terminals.
    /// </summary>
    public class TerminalConsoleRouter : ITerminalRouter<TerminalConsoleRouterContext>
    {
        /// <summary>
        /// Initialize a new <see cref="TerminalConsoleRouter"/> instance.
        /// </summary>
        /// <param name="terminalConsole">The terminal console.</param>
        /// <param name="applicationLifetime">The host application lifetime instance.</param>
        /// <param name="commandRouter">The command router.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public TerminalConsoleRouter(
            ITerminalConsole terminalConsole,
            IHostApplicationLifetime applicationLifetime,
            ICommandRouter commandRouter,
            ITerminalExceptionHandler exceptionHandler,
            TerminalOptions options,
            ILogger<TerminalConsoleRouter> logger)
        {
            this.terminalConsole = terminalConsole;
            this.applicationLifetime = applicationLifetime;
            this.commandRouter = commandRouter;
            this.exceptionHandler = exceptionHandler;
            this.options = options;
            this.logger = logger;
        }

        /// <summary>
        /// Runs to the terminal as a console asynchronously.
        /// </summary>
        /// <param name="context">The routing service context.</param>
        /// <returns></returns>
        public virtual async Task RunAsync(TerminalConsoleRouterContext context)
        {
            // Make sure we have supported start context
            if (context.StartContext.StartMode != TerminalStartMode.Console)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The requested start mode is not valid for console routing. start_mode={0}", context.StartContext.StartMode);
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
                    if (context.StartContext.TerminalCancellationToken.IsCancellationRequested)
                    {
                        throw new OperationCanceledException("Received terminal cancellation token, the terminal routing is canceled.");
                    }

                    // Check if application is stopping
                    if (applicationLifetime.ApplicationStopping.IsCancellationRequested)
                    {
                        throw new OperationCanceledException("Application is stopping, the terminal routing is canceled.");
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
                    CommandRouterContext routerContext = new(raw, context, properties: null);
                    route = routerContext.Route;
                    Task<CommandRouterResult> routeTask = commandRouter.RouteCommandAsync(routerContext);

                    bool success = routeTask.Wait(options.Router.Timeout, context.StartContext.TerminalCancellationToken);
                    if (!success)
                    {
                        throw new TimeoutException($"The command router timed out in {options.Router.Timeout} milliseconds.");
                    }
                }
                catch (OperationCanceledException oex)
                {
                    // Routing is canceled.
                    TerminalExceptionHandlerContext exContext = new(oex, route);
                    await exceptionHandler.HandleExceptionAsync(exContext);
                    break;
                }
                catch (Exception ex)
                {
                    // Task.Wait bundles up any exception into Exception.InnerException
                    TerminalExceptionHandlerContext exContext = new(ex.InnerException ?? ex, route);
                    await exceptionHandler.HandleExceptionAsync(exContext);
                }
            };
        }

        private readonly IHostApplicationLifetime applicationLifetime;
        private readonly ICommandRouter commandRouter;
        private readonly ITerminalExceptionHandler exceptionHandler;
        private readonly ILogger<TerminalConsoleRouter> logger;
        private readonly TerminalOptions options;
        private readonly ITerminalConsole terminalConsole;
    }
}
