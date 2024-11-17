/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Grpc.Core;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Runtime;
using System.Text.Json;
using System.Threading.Tasks;

namespace OneImlx.Terminal.AspNetCore
{
    /// <summary>
    /// Represents the gRPC service responsible for managing gRPC communication in the <c>OneImlx</c> terminal framework.
    /// </summary>
    public sealed class TerminalGrpcMapService : TerminalGrpcRouterProto.TerminalGrpcRouterProtoBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalGrpcMapService"/> class.
        /// </summary>
        /// <param name="terminalRouter">The terminal router instance for routing commands.</param>
        /// <param name="terminalProcessor">The terminal processor.</param>
        /// <param name="logger">The logger instance for logging router events and errors.</param>
        public TerminalGrpcMapService(
            ITerminalRouter<TerminalGrpcRouterContext> terminalRouter,
            ITerminalProcessor terminalProcessor,
            ILogger<TerminalGrpcMapService> logger)
        {
            this.terminalRouter = terminalRouter;
            this.terminalProcessor = terminalProcessor;
            this.logger = logger;
        }

        /// <summary>
        /// Routes the command string to be routed to an appropriate runner.
        /// </summary>
        /// <param name="request">The gRPC request containing the command string.</param>
        /// <param name="context">The gRPC server call context.</param>
        /// <returns>A task representing the asynchronous operation. Returns a response with the queued message items.</returns>
        /// <exception cref="TerminalException">Thrown when the terminal gRPC router is not running.</exception>
        /// <remarks>
        /// This method is designed to enqueue commands for processing by the terminal router's command queue. It
        /// expects a valid gRPC request containing a JSON body with the command string. The method extracts the command
        /// from the request body and adds it to the queue, associating it with the client information from the HTTP context.
        ///
        /// The method assumes that the terminal router is running and its command queue is initialized. If the queue is
        /// not active, a <see cref="TerminalException"/> is thrown to indicate that the router is not ready to process
        /// commands. Ensure the terminal router is correctly started before invoking this method.
        ///
        /// This method is primarily intended to be called by gRPC clients. It should not be invoked directly from
        /// within the application without proper context, as it depends on gRPC infrastructure and client context information.
        /// </remarks>
        public override async Task<TerminalGrpcRouterProtoOutput> RouteCommand(TerminalGrpcRouterProtoInput request, ServerCallContext context)
        {
            if (string.IsNullOrWhiteSpace(request.Request))
            {
                throw new TerminalException(TerminalErrors.MissingCommand, "The command is missing in the gRPC request.");
            }

            if (!terminalRouter.IsRunning)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The terminal gRPC router is not running.");
            }

            if (!terminalProcessor.IsProcessing)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The terminal processor is not processing.");
            }

            TerminalResponse response = await terminalProcessor.ProcessRequestAsync(request.Request, terminalProcessor.NewUniqueId(), context.Peer ?? "$unknown$");

            // Return the serialized byte array as part of the gRPC response.
            var output = new TerminalGrpcRouterProtoOutput
            {
                Response = terminalProcessor.SerializeToJsonString(response.Results)
            };
            return output;
        }

        // Private fields to hold injected dependencies and state information.
        private readonly ILogger<TerminalGrpcMapService> logger;
        private readonly ITerminalProcessor terminalProcessor;
        private readonly ITerminalRouter<TerminalGrpcRouterContext> terminalRouter;
    }
}
