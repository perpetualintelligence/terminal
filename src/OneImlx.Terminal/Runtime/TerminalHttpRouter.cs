/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Shared;

namespace OneImlx.Terminal.Runtime
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
        /// <param name="terminalProcessor">The terminal processing queue.</param>
        /// <param name="options">The options configuration for the terminal router.</param>
        /// <param name="logger">The logger instance for logging router events and errors.</param>
        public TerminalHttpRouter(
            ICommandRouter commandRouter,
            ITerminalExceptionHandler exceptionHandler,
            ITerminalProcessor terminalProcessor,
            IOptions<TerminalOptions> options,
            ILogger<TerminalHttpRouter> logger)
        {
            this.commandRouter = commandRouter;
            this.exceptionHandler = exceptionHandler;
            this.terminalProcessor = terminalProcessor;
            this.options = options;
            this.logger = logger;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="TerminalHttpRouter"/> is running.
        /// </summary>
        public bool IsRunning { get; protected set; }

        /// <summary>
        /// The terminal router name.
        /// </summary>
        public string Name => "http";

        /// <summary>
        /// Runs the HTTP router asynchronously and begins handling client requests indefinitely. The server will
        /// continue running until a cancellation is requested via the context.
        /// </summary>
        /// <param name="context">The terminal context containing configuration and cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation of running the server.</returns>
        /// <exception cref="TerminalException">Thrown when the start mode is not configured for HTTP.</exception>
        public async Task RunAsync(TerminalHttpRouterContext context)
        {
            if (context.StartMode != TerminalStartMode.Http)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "Invalid start mode for HTTP.");
            }

            try
            {
                logger.LogDebug("Terminal HTTP router started.");
                IsRunning = true;

                // Http is command and response so start command processing without background queue.
                terminalProcessor.StartProcessing(context, background: false);

                // Wait for the terminal to be canceled.
                await terminalProcessor.WaitUntilCanceledAsync(context.TerminalCancellationToken);
            }
            finally
            {
                await terminalProcessor.StopProcessingAsync(options.Value.Router.Timeout);
                logger.LogDebug("Terminal HTTP router stopped.");
                IsRunning = false;
            }
        }

        // Private fields to hold injected dependencies and state information.
        private readonly ICommandRouter commandRouter;
        private readonly ITerminalExceptionHandler exceptionHandler;
        private readonly ILogger<TerminalHttpRouter> logger;
        private readonly IOptions<TerminalOptions> options;
        private readonly ITerminalProcessor terminalProcessor;
    }
}
