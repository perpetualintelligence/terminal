/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;

namespace OneImlx.Terminal.AspNetCore.Runtime
{
    /// <summary>
    /// Represents the HTTP router responsible for managing HTTP communication in the terminal. This router handles
    /// incoming HTTP commands and routes them to the appropriate command runners.
    /// </summary>
    public class TerminalHttpRouter : ITerminalRouter<TerminalHttpRouterContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalHttpRouter"/> class.
        /// </summary>
        /// <param name="commandRouter">The command router for routing commands to specific handlers.</param>
        /// <param name="exceptionHandler">The exception handler for handling errors that occur during command routing.</param>
        /// <param name="options">The options configuration for the terminal router.</param>
        /// <param name="logger">The logger instance for logging router events and errors.</param>
        public TerminalHttpRouter(
            ICommandRouter commandRouter,
            ITerminalExceptionHandler exceptionHandler,
            IOptions<TerminalOptions> options,
            ILogger<TerminalHttpRouter> logger)
        {
            this.commandRouter = commandRouter;
            this.exceptionHandler = exceptionHandler;
            this.options = options;
            this.logger = logger;
        }

        /// <summary>
        /// The command queue for the terminal router.
        /// </summary>
        public TerminalRemoteMessageQueue? CommandQueue => commandQueue;

        /// <summary>
        /// Runs the HTTP router asynchronously and begins handling client requests indefinitely. The server will
        /// continue running until a cancellation is requested via the context.
        /// </summary>
        /// <param name="context">The terminal context containing configuration and cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation of running the server.</returns>
        /// <exception cref="TerminalException">Thrown when the start mode is not configured for HTTP.</exception>
        public async Task RunAsync(TerminalHttpRouterContext context)
        {
            if (context.StartContext.StartMode != TerminalStartMode.Http)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "Invalid start mode for HTTP.");
            }

            // Initialize the command queue for remote message processing.
            commandQueue = new TerminalRemoteMessageQueue(commandRouter, exceptionHandler, options.Value, context, logger);
            try
            {
                logger.LogDebug("Terminal HTTP router started.");

                // Start background command processing and block the current thread.
                await commandQueue.StartCommandProcessingAsync();
            }
            finally
            {
                logger.LogDebug("Terminal HTTP router stopped.");
            }
        }

        // Private fields to hold injected dependencies and state information.
        private readonly ICommandRouter commandRouter;
        private readonly ITerminalExceptionHandler exceptionHandler;
        private readonly ILogger<TerminalHttpRouter> logger;
        private readonly IOptions<TerminalOptions> options;
        private TerminalRemoteMessageQueue? commandQueue;
    }
}
