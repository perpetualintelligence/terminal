/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneImlx.Terminal.AspNetCore
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
        /// <param name="logger">The logger instance for logging router events and errors.</param>
        public TerminalHttpMapService(
            ITerminalRouter<TerminalHttpRouterContext> terminalRouter,
            ILogger<TerminalHttpMapService> logger)
        {
            this.terminalRouter = terminalRouter;
            this.logger = logger;
        }

        /// <summary>
        /// Routes the command string to an appropriate runner via HTTP.
        /// </summary>
        /// <param name="context">The HTTP context containing the request.</param>
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
        public async Task<IEnumerable<TerminalRemoteQueueRequest>> RouteCommandAsync(HttpContext context)
        {
            if (terminalRouter.CommandQueue == null)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The terminal HTTP router is not running.");
            }

            // Read the JSON body from the HTTP request and deserialize it into the TerminalHttpRequest object.
            TerminalJsonCommandRequest? request;
            using (var reader = new StreamReader(context.Request.Body))
            {
                var requestBody = await reader.ReadToEndAsync();
                request = JsonSerializer.Deserialize<TerminalJsonCommandRequest>(requestBody);
            }

            if (request == null || string.IsNullOrWhiteSpace(request.CommandString))
            {
                throw new TerminalException(TerminalErrors.MissingCommand, "The command is missing in the HTTP request.");
            }

            // Enqueue the command string. The command is queued along with the client's IP address from the HTTP context.
            string peer = context.Connection.RemoteIpAddress?.ToString() ?? "$unknown$";
            return terminalRouter.CommandQueue.Enqueue(request.CommandString, peer, Guid.NewGuid().ToString());
        }

        // Private fields to hold injected dependencies and state information.
        private readonly ILogger<TerminalHttpMapService> logger;
        private readonly ITerminalRouter<TerminalHttpRouterContext> terminalRouter;
    }
}
