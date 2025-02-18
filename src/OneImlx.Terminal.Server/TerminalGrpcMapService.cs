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

namespace OneImlx.Terminal.Server
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
        /// Routes the <see cref="TerminalGrpcRouterProtoInput"/> to the appropriate command runner.
        /// </summary>
        /// <param name="protoInput">The gRPC input containing the <see cref="TerminalInputOutput"/>.</param>
        /// <param name="context">The gRPC server call context.</param>
        /// <returns>A task representing the asynchronous operation, including the <see cref="TerminalGrpcRouterProtoOutput"/>.</returns>
        /// <exception cref="TerminalException">Thrown when the terminal gRPC router is not running.</exception>
        /// <remarks>
        /// Application code should not call this method directly. Instead, use the client NuGet package to send the
        /// command to the gRPC terminal server.
        /// </remarks>
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
            TerminalInputOutput? input = JsonSerializer.Deserialize<TerminalInputOutput>(protoInput.InputJson);
            if (input == null || input.Count <= 0)
            {
                throw new TerminalException(TerminalErrors.MissingCommand, "The input requests are missing in the gRPC route.");
            }

            // Execute the commands
            await terminalProcessor.ExecuteAsync(input);

            // Return the terminal output to the client.
            var protoOutput = new TerminalGrpcRouterProtoOutput
            {
                OutputJson = JsonSerializer.Serialize(input)
            };
            return protoOutput;
        }

        // Private fields to hold injected dependencies and state information.
        private readonly ILogger<TerminalGrpcMapService> logger;
        private readonly ITerminalProcessor terminalProcessor;
        private readonly ITerminalRouter<TerminalGrpcRouterContext> terminalRouter;
    }
}
