/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// Represents the gRPC router responsible for managing gRPC communication in the terminal. This router handles
    /// incoming gRPC commands and routes them to the appropriate command runners.
    /// </summary>
    public class TerminalGrpcRouter : ITerminalRouter<TerminalGrpcRouterContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalGrpcRouter"/> class.
        /// </summary>
        /// <param name="terminalProcessor">The terminal router queue.</param>
        /// <param name="options">The options configuration for the terminal router.</param>
        /// <param name="logger">The logger instance for logging router events and errors.</param>
        public TerminalGrpcRouter(
            ITerminalProcessor terminalProcessor,
            IOptions<TerminalOptions> options,
            ILogger<TerminalGrpcRouter> logger)
        {
            this.terminalProcessor = terminalProcessor;
            this.options = options;
            this.logger = logger;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="TerminalGrpcRouter"/> is running.
        /// </summary>
        public bool IsRunning { get; protected set; }

        /// <summary>
        /// Runs the gRPC server asynchronously and begins handling client requests indefinitely. The server will
        /// continue running until a cancellation is requested via the context.
        /// </summary>
        /// <param name="context">The terminal context containing configuration and cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation of running the server.</returns>
        /// <exception cref="TerminalException">Thrown when the start mode is not configured for gRPC.</exception>
        public async Task RunAsync(TerminalGrpcRouterContext context)
        {
            if (context.StartContext.StartMode != TerminalStartMode.Grpc)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "Invalid start mode for gRPC.");
            }

            // Initialize the command queue for remote message processing.
            try
            {
                logger.LogDebug("Terminal gRPC router started.");
                IsRunning = true;

                // Start background command processing and blocking the current thread.
                terminalProcessor.StartProcessing(context);

                // Run indefinitely until the cancellation token is triggered
                await terminalProcessor.WaitAsync(context);
            }
            finally
            {
                await terminalProcessor.StopProcessingAsync(options.Value.Router.Timeout);
                logger.LogInformation("Terminal gRPC router stopped.");
                IsRunning = false;
            }
        }

        // Private fields to hold injected dependencies and state information.
        private readonly ILogger<TerminalGrpcRouter> logger;
        private readonly IOptions<TerminalOptions> options;
        private readonly ITerminalProcessor terminalProcessor;
    }
}
