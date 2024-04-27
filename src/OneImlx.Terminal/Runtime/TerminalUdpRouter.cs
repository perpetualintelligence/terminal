/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalRouter{TContext}"/> for UDP server communication.
    /// </summary>
    /// <remarks>
    /// <para>This class implements the <see cref="ITerminalRouter{TContext}"/> interface and is responsible for handling
    /// UDP server communication. It runs a terminal as a UDP server on the specified IP endpoint and listens for incoming UDP datagrams.
    /// Unlike TCP, UDP is connectionless, allowing this server to receive messages from multiple clients without establishing a
    /// dedicated connection for each. Messages can be received and processed concurrently from various sources, making UDP suitable
    /// for scenarios where low latency or high-throughput communication is required, albeit with the trade-off of reliability.</para>
    /// <para>The server can be gracefully stopped by canceling the provided cancellation token in the context, ensuring any ongoing
    /// operations are terminated and resources are cleanly released.</para>
    /// </remarks>
    /// <seealso cref="TerminalConsoleRouter"/>
    /// <seealso cref="TerminalTcpRouter"/>
    public sealed class TerminalUdpRouter : ITerminalRouter<TerminalUdpRouterContext>
    {
        private readonly ICommandRouter commandRouter;
        private readonly ITerminalExceptionHandler exceptionHandler;
        private readonly TerminalOptions options;
        private readonly ITerminalTextHandler textHandler;
        private readonly ILogger<TerminalUdpRouter> logger;
        private UdpClient? _udpClient;
        private TerminalCommandQueue? commandQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalUdpRouter"/> class.
        /// </summary>
        /// <param name="commandRouter">The command router to route commands to.</param>
        /// <param name="exceptionHandler">The handler for exceptions that occur during routing.</param>
        /// <param name="options">Configuration options for the terminal.</param>
        /// <param name="textHandler">The handler for processing text data.</param>
        /// <param name="logger">The logger for logging information and errors.</param>
        public TerminalUdpRouter(
            ICommandRouter commandRouter,
            ITerminalExceptionHandler exceptionHandler,
            TerminalOptions options,
            ITerminalTextHandler textHandler,
            ILogger<TerminalUdpRouter> logger)
        {
            this.commandRouter = commandRouter;
            this.exceptionHandler = exceptionHandler;
            this.options = options;
            this.textHandler = textHandler;
            this.logger = logger;
        }

        /// <summary>
        /// Asynchronously runs the UDP router, listening for incoming UDP packets and processing them.
        /// </summary>
        /// <param name="context">The UDP router context containing configuration and runtime information.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task RunAsync(TerminalUdpRouterContext context)
        {
            // Set up the command queue
            commandQueue = new TerminalCommandQueue(commandRouter, exceptionHandler, options, context, logger);

            if (context.StartContext.StartMode != TerminalStartMode.Udp)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The requested start mode is not valid for UDP routing. start_mode={0}", context.StartContext.StartMode);
            }

            if (context.IPEndPoint == null)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The network IP endpoint is missing in the UDP server routing request.");
            }

            try
            {
                _udpClient = new UdpClient(context.IPEndPoint);
                logger.LogDebug($"Terminal UDP router started. endpoint={context.IPEndPoint}");

                // Start processing the command queue immediately in the background. The code starts the background task for processing commands immediately
                // and does not wait for it to complete. The _ = discards the returned task since we don't need to await it in this context. It effectively
                // runs in the background, processing commands as they are enqueued.
                Task backgroundProcessingTask = commandQueue.StartCommandProcessing();

                // Enqueue the UDP data packets till a terminal cancellation is requested.
                while (!context.StartContext.TerminalCancellationToken.IsCancellationRequested)
                {
                    // Ensure that application can respond to a cancellation request even when waiting for
                    // incoming UDP packets. We need to support .NET Framework 4.8.1, .NET Standard, and .NET 8+, and since _udpClient.ReceiveAsync()
                    // does not accept a CancellationToken directly in all these versions, we implement a workaround using Task.WhenAny to respect the cancellation token.
                    Task<UdpReceiveResult> receiveTask = _udpClient.ReceiveAsync();
                    Task receiveOrCancellationTask = await Task.WhenAny(receiveTask, Task.Delay(Timeout.Infinite, context.StartContext.TerminalCancellationToken));

                    if (receiveOrCancellationTask == receiveTask)
                    {
                        // Process received data
                        UdpReceiveResult receivedResult = receiveTask.Result; // Safe because we know the task has completed
                        string receivedText = textHandler.Encoding.GetString(receivedResult.Buffer);
                        commandQueue.Enqueue(receivedText, receivedResult.RemoteEndPoint);
                    }
                    else
                    {
                        // Cancellation token has been triggered
                        logger.LogDebug("Terminal UDP router cancelled.");
                        break;
                    }
                }

                // If we are here then that means a cancellation is requested, gracefully stop the background process.
                Task routerTimeout = Task.Delay(options.Router.Timeout);
                var completedTask = await Task.WhenAny(backgroundProcessingTask, routerTimeout);
                if (completedTask == routerTimeout)
                {
                    logger.LogError("Command processing task failed to complete. timeout={0}", options.Router.Timeout);
                }
                else
                {
                    logger.LogDebug("Command processing task completed.");
                }
            }
            catch (Exception ex)
            {
                await exceptionHandler.HandleExceptionAsync(new TerminalExceptionHandlerContext(ex, null));
            }
            finally
            {
                _udpClient?.Close();
                logger.LogDebug("Terminal UDP router stopped. endpoint={0}", context.IPEndPoint);
            }
        }
    }
}