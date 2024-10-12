/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grpc.Core;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// Represents the gRPC service responsible for managing gRPC communication in the <c>OneImlx</c> terminal
    /// framework. This router handles incoming gRPC commands and routes them to the appropriate command runners.
    /// </summary>
    public class TerminalGrpcMapService : OneImlxGrpcRouterInternal.OneImlxGrpcRouterInternalBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalGrpcRouter"/> class.
        /// </summary>
        /// <param name="terminalRouter"></param>
        /// <param name="logger">The logger instance for logging router events and errors.</param>
        public TerminalGrpcMapService(
            ITerminalRouter<TerminalGrpcRouterContext> terminalRouter,
            ILogger<TerminalGrpcMapService> logger)
        {
            this.terminalRouter = terminalRouter;
            this.logger = logger;
        }

        /// <summary>
        /// Enqueues the command string to be routed to an appropriate runner.
        /// </summary>
        /// <param name="request">The gRPC request containing the command string.</param>
        /// <param name="context">The gRPC server call context.</param>
        /// <returns>A task representing the asynchronous operation. Returns an empty response.</returns>
        /// <exception cref="TerminalException">Thrown when the terminal gRPC router is not running.</exception>
        /// <remarks>
        /// This method is designed to enqueue commands for processing by the terminal router's command queue. It
        /// expects a valid gRPC request containing a command string. The method extracts the command and adds it to the
        /// queue, associating it with the peer information from the gRPC context.
        ///
        /// The method assumes that the terminal router is running and its command queue is initialized. If the queue is
        /// not active, a <see cref="TerminalException"/> is thrown to indicate that the router is not ready to process
        /// commands. Ensure the terminal router is correctly started before invoking this method.
        ///
        /// This method is primarily intended to be called by gRPC clients. It should not be invoked directly from
        /// within the application without proper context, as it depends on gRPC infrastructure and client context information.
        /// </remarks>
        public override Task<OneImlxGrpcRouterResponseInternal> EnqueueCommandInternal(OneImlxGrpcRouterRequestInternal request, ServerCallContext context)
        {
            if (terminalRouter.CommandQueue == null)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The terminal gRPC router is not running.");
            }

            // Enqueue the command string. The command is queued along with the peer information from the context.
            terminalRouter.CommandQueue.Enqueue(request.Request, context.Peer, Guid.NewGuid().ToString());

            // Return an empty response as per the gRPC protocol definition.
            return Task.FromResult(new OneImlxGrpcRouterResponseInternal() { Response = context.Peer });
        }

        // Private fields to hold injected dependencies and state information.
        private readonly ILogger<TerminalGrpcMapService> logger;
        private readonly ITerminalRouter<TerminalGrpcRouterContext> terminalRouter;
    }
}
