/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Runtime;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Server
{
    /// <summary>
    /// Represents the HTTP service responsible for managing HTTP communication in the <c>OneImlx</c> terminal
    /// framework. This router handles incoming HTTP commands and routes them to the appropriate command runners.
    /// </summary>
    public sealed class TerminalHttpMapService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalHttpMapService"/> class.
        /// </summary>
        /// <param name="terminalRouter">The terminal router.</param>
        /// <param name="terminalProcessor">The terminal processor.</param>
        /// <param name="logger">The logger instance for logging router events and errors.</param>
        public TerminalHttpMapService(
            ITerminalRouter<TerminalHttpRouterContext> terminalRouter,
            ITerminalProcessor terminalProcessor,
            ILogger<TerminalHttpMapService> logger)
        {
            this.terminalRouter = terminalRouter;
            this.terminalProcessor = terminalProcessor;
            this.logger = logger;
        }

        /// <summary>
        /// Routes the <see cref="TerminalInput"/> via HTTP.
        /// </summary>
        /// <param name="httpContext">The HTTP context containing the request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="TerminalException">Thrown when the terminal HTTP router is not running.</exception>
        /// <remarks>
        /// This method is designed to enqueue commands for processing by the terminal router's command queue. It
        /// expects a valid HTTP request containing a JSON body with the command string. The method extracts the command
        /// from the request body and adds it to the queue, associating it with the client information from the HTTP context.
        ///
        /// The method assumes that the terminal router is running and its command queue is initialized. If the queue is
        /// not active, a <see cref="TerminalException"/> is thrown to indicate that the router is not ready to process
        /// commands. Ensure the terminal router is correctly started before invoking this method.
        ///
        /// This method is primarily intended to be called by HTTP clients. It should not be invoked directly from
        /// within the application without proper context, as it depends on HTTP infrastructure and client context information.
        /// </remarks>
        public async Task RouteAsync(HttpContext httpContext)
        {
            if (!terminalRouter.IsRunning)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The terminal HTTP router is not running.");
            }

            if (!terminalProcessor.IsProcessing)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The terminal processor is not processing.");
            }

            TerminalInput? input = await httpContext.Request.ReadFromJsonAsync<TerminalInput>();
            if (input == null || input.Count <= 0)
            {
                throw new TerminalException(TerminalErrors.MissingCommand, "The input is missing in the HTTP request.");
            }

            string? clientIp = httpContext.Connection.RemoteIpAddress?.ToString();
            TerminalOutput? output = await terminalProcessor.ExecuteAsync(input, senderId: null, clientIp);
            await httpContext.Response.WriteAsJsonAsync(output);
        }

        // Private fields to hold injected dependencies and state information.
        private readonly ILogger<TerminalHttpMapService> logger;
        private readonly ITerminalProcessor terminalProcessor;
        private readonly ITerminalRouter<TerminalHttpRouterContext> terminalRouter;
    }
}
