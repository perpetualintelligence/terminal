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

        /// commands. Ensure the terminal router is correctly started before invoking this method.
        ///
        /// This method is primarily intended to be called by gRPC clients. It should not be invoked directly from
        /// within the application without proper context, as it depends on gRPC infrastructure and client context
        /// information. </remarks>
        public override async Task<TerminalGrpcRouterProtoOutput> RouteCommand(TerminalGrpcRouterProtoInput protoInput, ServerCallContext context)
        {
            if (!terminalRouter.IsRunning)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The terminal gRPC router is not running.");
            }

            if (!terminalProcessor.IsProcessing)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The terminal processor is not processing.");
            }

            // Convert to terminal input.
            TerminalInput? input = JsonSerializer.Deserialize<TerminalInput>(protoInput.InputJson) ?? throw new TerminalException(TerminalErrors.ServerError, "The terminal JSON input is invalid.");
            TerminalOutput output = await terminalProcessor.ExecuteAsync(input, senderId: null, senderEndpoint: null);

            // Return the terminal output to the client.
            var protoOutput = new TerminalGrpcRouterProtoOutput
            {
                OutputJson = JsonSerializer.Serialize(output)
            };
            return protoOutput;
        }

        // Private fields to hold injected dependencies and state information.
        private readonly ILogger<TerminalGrpcMapService> logger;
        private readonly ITerminalProcessor terminalProcessor;
        private readonly ITerminalRouter<TerminalGrpcRouterContext> terminalRouter;
    }
}
