/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Shared;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalRouter{TContext}"/> for UDP server communication.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class implements the <see cref="ITerminalRouter{TContext}"/> interface and is responsible for handling UDP
    /// server communication. It runs a terminal as a UDP server on the specified IP endpoint and listens for incoming
    /// UDP datagrams. Unlike TCP, UDP is connectionless, allowing this server to receive messages from multiple clients
    /// without establishing a dedicated connection for each. Messages can be received and processed concurrently from
    /// various sources, making UDP suitable for scenarios where low latency or high-throughput communication is
    /// required, albeit with the trade-off of reliability.
    /// </para>
    /// <para>
    /// The server can be gracefully stopped by canceling the provided cancellation token in the context, ensuring any
    /// ongoing operations are terminated and resources are cleanly released.
    /// </para>
    /// </remarks>
    /// <seealso cref="TerminalConsoleRouter"/>
    /// <seealso cref="TerminalTcpRouter"/>
    public class TerminalUdpRouter : ITerminalRouter<TerminalUdpRouterContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalUdpRouter"/> class.
        /// </summary>
        /// <param name="exceptionHandler">The handler for exceptions that occur during routing.</param>
        /// <param name="terminalOptions">Configuration options for the terminal.</param>
        /// <param name="textHandler">The handler for processing text data.</param>
        /// <param name="terminalProcessor">The terminal processing queue.</param>
        /// <param name="logger">The logger for logging information and errors.</param>
        public TerminalUdpRouter(
            ITerminalExceptionHandler exceptionHandler,
            IOptions<TerminalOptions> terminalOptions,
            ITerminalTextHandler textHandler,
            ITerminalProcessor terminalProcessor,
            ILogger<TerminalUdpRouter> logger)
        {
            this.exceptionHandler = exceptionHandler;
            this.terminalOptions = terminalOptions;
            this.textHandler = textHandler;
            this.terminalProcessor = terminalProcessor;
            this.logger = logger;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="TerminalUdpRouter"/> is running.
        /// </summary>
        public bool IsRunning { get; protected set; }

        /// <summary>
        /// The terminal router name.
        /// </summary>
        public string Name => "udp";

        /// <summary>
        /// Asynchronously runs the UDP router, listening for incoming UDP packets and processing them.
        /// </summary>
        /// <param name="context">The UDP router context containing configuration and runtime information.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task RunAsync(TerminalUdpRouterContext context)
        {
            if (context.StartMode != TerminalStartMode.Udp)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The requested start mode is not valid for UDP routing. start_mode={0}", context.StartMode);
            }

            if (context.IPEndPoint == null)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The network IP endpoint is missing in the UDP server routing request.");
            }

            this.udpClient = new(context.IPEndPoint);

            try
            {
                logger.LogDebug($"Terminal UDP router started. endpoint={context.IPEndPoint}");
                IsRunning = true;

                // Start processing the command queue immediately in the background. The code starts the background task
                // for processing commands immediately and does not wait for it to complete. The _ = discards the
                // returned task since we don't need to await it in this context. It effectively runs in the background,
                // processing commands as they are enqueued.
                terminalProcessor.StartProcessing(context, background: true, responseHandler: HandleResponseAsync);
                while (true)
                {
                    // Await either the receive task or a cancellation.
                    var receiveTask = udpClient.ReceiveAsync();
                    await Task.WhenAny(receiveTask, Task.Delay(Timeout.Infinite, context.TerminalCancellationToken));

                    // Process received data if the receive task completes.
                    if (receiveTask.Status == TaskStatus.RanToCompletion)
                    {
                        UdpReceiveResult receivedResult = receiveTask.Result;
                        await terminalProcessor.StreamAsync(receivedResult.Buffer, receivedResult.Buffer.Length, receivedResult.RemoteEndPoint.ToString(), receivedResult.RemoteEndPoint.ToString());
                    }
                    else
                    {
                        // Throw cancellation exception so its handled by the exception handler.
                        context.TerminalCancellationToken.ThrowIfCancellationRequested();
                    }
                }
            }
            catch (Exception ex)
            {
                await exceptionHandler.HandleExceptionAsync(new TerminalExceptionHandlerContext(ex, null));
            }
            finally
            {
                udpClient?.Close();
                await terminalProcessor.StopProcessingAsync(terminalOptions.Value.Router.Timeout);
                logger.LogDebug("Terminal UDP router stopped. endpoint={0}", context.IPEndPoint);
                IsRunning = false;
            }
        }

        private async Task HandleResponseAsync(TerminalInputOutput output)
        {
            try
            {
                if (output.SenderEndpoint == null)
                {
                    throw new TerminalException(TerminalErrors.ServerError, "The sender endpoint is missing in the response.");
                }

                if (udpClient == null)
                {
                    throw new TerminalException(TerminalErrors.ServerError, "The UDP client is not initialized.");
                }

                // Split the string
                string[] parts = output.SenderEndpoint.Split(':');
                if (parts.Length != 2 || !int.TryParse(parts[1], out int port))
                {
                    throw new TerminalException(TerminalErrors.ServerError, "The sender endpoint has an invalid format.");
                }

                IPAddress ipAddress = IPAddress.Parse(parts[0]);
                IPEndPoint senderEndpoint = new(ipAddress, port);

                byte[] responseBytes = TerminalServices.DelimitBytes(JsonSerializer.SerializeToUtf8Bytes(output), terminalOptions.Value.Router.StreamDelimiter);
                await udpClient.SendAsync(responseBytes, responseBytes.Length, senderEndpoint);
            }
            catch (Exception ex)
            {
                await exceptionHandler.HandleExceptionAsync(new TerminalExceptionHandlerContext(ex, null));
            }
        }

        private readonly ITerminalExceptionHandler exceptionHandler;
        private readonly ILogger<TerminalUdpRouter> logger;
        private readonly IOptions<TerminalOptions> terminalOptions;
        private readonly ITerminalProcessor terminalProcessor;
        private readonly ITerminalTextHandler textHandler;
        private UdpClient? udpClient;
    }
}
