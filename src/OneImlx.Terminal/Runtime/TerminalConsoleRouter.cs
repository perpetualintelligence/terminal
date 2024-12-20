﻿/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Configuration.Options;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalRouter{TContext}"/> for console based terminals.
    /// </summary>
    public sealed class TerminalConsoleRouter : ITerminalRouter<TerminalConsoleRouterContext>
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
        /// Gets a value indicating whether the console terminal is running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Runs to the terminal as a console asynchronously.
        /// </summary>
        /// <param name="context">The routing service context.</param>
        /// <returns></returns>
        public async Task RunAsync(TerminalConsoleRouterContext context)
        {
            // Make sure we have supported start context
            if (context.StartContext.StartMode != TerminalStartMode.Console)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The requested start mode is not valid for console routing. start_mode={0}", context.StartContext.StartMode);
            }

            // Track the application lifetime so we can know whether cancellation is requested.
            try
            {
                IsRunning = true;
                while (true)
                {
                    TerminalRequest? request = null;

                    try
                    {
                        // Wait for a bit to avoid CPU hogging and give time for cancellation token to be set.
                        await Task.Delay(options.Router.RouteDelay);

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

                        // Read the user input and ignore empty commands
                        string? raw = await terminalConsole.ReadLineAsync();
                        if (terminalConsole.Ignore(raw))
                        {
                            // Wait for next command.
                            logger.LogDebug("The raw string is null or ignored by the terminal console.");
                            continue;
                        }

                        // Execute the command asynchronously
                        request = new(Guid.NewGuid().ToString(), raw!);
                        CommandContext routerContext = new(request, context, properties: null);
                        var routeTask = commandRouter.RouteCommandAsync(routerContext);
                        if (await Task.WhenAny(routeTask, Task.Delay(options.Router.Timeout)) != routeTask)
                        {
                            throw new TimeoutException($"The command router timed out in {options.Router.Timeout} milliseconds.");
                        }

                        // Process the result
                        var result = await routeTask;
                    }
                    catch (OperationCanceledException oex)
                    {
                        // Routing is canceled.
                        TerminalExceptionHandlerContext exContext = new(oex, request);
                        await exceptionHandler.HandleExceptionAsync(exContext);
                        break;
                    }
                    catch (Exception ex)
                    {
                        // Task.Wait bundles up any exception into Exception.InnerException
                        TerminalExceptionHandlerContext exContext = new(ex.InnerException ?? ex, request);
                        await exceptionHandler.HandleExceptionAsync(exContext);
                    }
                };
            }
            finally
            {
                IsRunning = false;
            }
        }

        private readonly IHostApplicationLifetime applicationLifetime;
        private readonly ICommandRouter commandRouter;
        private readonly ITerminalExceptionHandler exceptionHandler;
        private readonly ILogger<TerminalConsoleRouter> logger;
        private readonly TerminalOptions options;
        private readonly ITerminalConsole terminalConsole;
    }
}
