/*
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
using OneImlx.Terminal.Shared;

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
        /// <param name="commandRouter">The command router.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public TerminalConsoleRouter(
            ITerminalConsole terminalConsole,
            ICommandRouter commandRouter,
            ITerminalExceptionHandler exceptionHandler,
            TerminalOptions options,
            ILogger<TerminalConsoleRouter> logger)
        {
            this.terminalConsole = terminalConsole;
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
        /// The terminal router name.
        /// </summary>
        public string Name => "console";

        /// <summary>
        /// Runs to the terminal as a console asynchronously.
        /// </summary>
        /// <param name="context">The routing service context.</param>
        /// <returns></returns>
        public async Task RunAsync(TerminalConsoleRouterContext context)
        {
            // Make sure we have supported start context
            if (context.StartMode != TerminalStartMode.Console)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The requested start mode is not valid for console routing. start_mode={0}", context.StartMode);
            }

            // Track the application lifetime so we can know whether cancellation is requested.
            try
            {
                IsRunning = true;
                bool routeOnce = context.RouteOnce.GetValueOrDefault();
                bool routed = false;

                while (true)
                {
                    TerminalRequest? request = null;

                    try
                    {
                        // Wait for a bit to avoid CPU hogging and give time for cancellation token to be set.
                        await Task.Delay(options.Router.RouteDelay);

                        // Honor the cancellation request.
                        if (context.TerminalCancellationToken.IsCancellationRequested)
                        {
                            throw new OperationCanceledException("Received terminal cancellation token, the terminal console router is canceled.");
                        }

                        // Route once handling for driver programs or indefinite routing for interactive terminals.
                        string? raw = null;
                        if (routeOnce)
                        {
                            // Route once is only valid for driver programs.
                            if (!options.Driver.Enabled)
                            {
                                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The route once is only valid for driver programs.");
                            }

                            // Driver programs executes the program in its entirety. So once the request is routed the
                            // router should stop, even if there was an error.
                            if (routed)
                            {
                                logger.LogDebug("The driver request is routed once, the terminal console router is complete.");
                                break;
                            }

                            // Not yet routed, so route the driver program.
                            string[] args = context.Arguments ?? [];
                            if (args != null)
                            {
                                if (args.Length != 0)
                                {
                                    raw = $"{options.Driver.RootId}{options.Parser.Separator}{args[0]}";
                                }
                                else
                                {
                                    raw = options.Driver.RootId;
                                }
                            }
                        }
                        else
                        {
                            // Print the caret and read the user input.
                            if (options.Router.Caret != null)
                            {
                                await terminalConsole.WriteAsync(options.Router.Caret);
                            }
                            raw = await terminalConsole.ReadLineAsync();
                        }

                        // Determine if the raw string is to be ignored.
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
                            throw new TimeoutException($"The terminal console router timed out in {options.Router.Timeout} milliseconds.");
                        }

                        // Process the result. If this is a driver program then we terminate the loop.
                        CommandResult result = await routeTask;
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
                    finally
                    {
                        routed = true;
                    }
                }
                ;
            }
            finally
            {
                IsRunning = false;
            }
        }

        private readonly ICommandRouter commandRouter;
        private readonly ITerminalExceptionHandler exceptionHandler;
        private readonly ILogger<TerminalConsoleRouter> logger;
        private readonly TerminalOptions options;
        private readonly ITerminalConsole terminalConsole;
    }
}
