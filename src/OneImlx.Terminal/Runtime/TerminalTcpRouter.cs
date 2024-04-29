/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalRouter{TContext}"/> for TCP client-server communication.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class implements the <see cref="ITerminalRouter{TContext}"/> interface and is responsible for handling TCP
    /// client-server communication. It runs a terminal as a TCP server on the specified endpoint and waits for incoming
    /// client connections. The server can be gracefully stopped by triggering a cancellation token in the context.
    /// </para>
    /// </remarks>
    /// <seealso cref="TerminalUdpRouter"/>
    /// <seealso cref="TerminalConsoleRouter"/>
    public class TerminalTcpRouter : ITerminalRouter<TerminalTcpRouterContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalTcpRouter"/> class.
        /// </summary>
        /// <param name="commandRouter">The command router.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="logger">The logger.</param>
        /// <remarks>
        /// This constructor creates a new instance of the <see cref="TerminalTcpRouter"/> class. It takes several
        /// dependencies that are required for handling TCP client-server communication.
        /// </remarks>
        public TerminalTcpRouter(
            ICommandRouter commandRouter,
            ITerminalExceptionHandler exceptionHandler,
            TerminalOptions options,
            ITerminalTextHandler textHandler,
            ILogger<TerminalTcpRouter> logger)
        {
            this.commandRouter = commandRouter;
            this.exceptionHandler = exceptionHandler;
            this.options = options;
            this.textHandler = textHandler;
            this.logger = logger;
        }

        /// <summary>
        /// Runs the TCP server for handling client connections asynchronously.
        /// </summary>
        /// <param name="context">The routing context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method starts a TCP server on the specified IP endpoint and waits for incoming client connections. It
        /// handles the client connections asynchronously by creating a task for each incoming connection. The server
        /// can be gracefully stopped by canceling the provided cancellation token in the context. The method will also
        /// stop if an exception is encountered while handling client connections.
        /// </remarks>
        public virtual async Task RunAsync(TerminalTcpRouterContext context)
        {
            // Reset the command queue
            commandQueue = new TerminalRemoteMessageQueue(commandRouter, exceptionHandler, options, context, logger);
            connectionLimiter = new SemaphoreSlim(options.Router.MaxRemoteClients);

            // Ensure we have supported start context
            if (context.StartContext.StartMode != TerminalStartMode.Tcp)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The requested start mode is not valid for TCP routing. start_mode={0}", context.StartContext.StartMode);
            }

            // Ensure we have a valid IP endpoint
            if (context.IPEndPoint == null)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The network IP endpoint is missing in the TCP server routing request.");
            }

            _server = new(context.IPEndPoint);
            try
            {
                // Start the TCP server
                _server.Start();
                logger.LogDebug("Terminal TCP routing started. endpoint={0}", context.IPEndPoint);

                // Start processing the command queue immediately in the background. The code starts the background task
                // for processing commands immediately and does not wait for it to complete. The _ = discards the
                // returned task since we don't need to await it in this context. It effectively runs in the background,
                // processing commands as they are enqueued.
                Task backgroundProcessingTask = commandQueue.StartCommandProcessing();

                // We have initialized the requested client connections. Now we wait for all the client connections to
                // complete until the cancellation requested. Each time a a client task complete a new task is created
                // to accept a new client connection.
                await AcceptClientsUntilCanceledAsync(context);

            }
            catch (Exception ex)
            {
                // The default exception handler will log the exception and return a generic error message to the client.
                await exceptionHandler.HandleExceptionAsync(new TerminalExceptionHandlerContext(ex, null));
            }
            finally
            {
                _server.Stop(); // Stop the TCP server
                logger.LogDebug("Terminal TCP routing stopped. endpoint={0}", context.IPEndPoint);
            }
        }

        private async Task AcceptClientsUntilCanceledAsync(TerminalTcpRouterContext context)
        {
            // Ensure the server is initialized
            if (_server == null || connectionLimiter == null)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The TCP server is not initialized.");
            }

            // The server is running and accepting client connections. We will wait for incoming client connections
            while (!context.StartContext.TerminalCancellationToken.IsCancellationRequested)
            {
                // Wait for the semaphore to allow a new connection.
                await connectionLimiter.WaitAsync(context.StartContext.TerminalCancellationToken);

                // The command processing happens in the background till client is disconnected or the cancellation
                // token is triggered.
                Task clientAcceptTask = Task.Run(async () =>
                {
                    // Once the client is accepted this is the client id. The semaphore ensures we have a limited number
                    // of clients connected.
                    string clientId = Guid.NewGuid().ToString();

                    try
                    {
                        using (TcpClient client = await _server.AcceptTcpClientAsync())
                        {
                            logger.LogError("Client connected. endpoint={0} id={1}", client.Client.RemoteEndPoint, clientId);
                            await HandleClientConnectedAsync(context, client, clientId);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Release the semaphore in case of an exception.
                        connectionLimiter.Release();
                        await HandleClientExceptionAsync(ex, clientId);
                    }

                    // No exception, release the semaphore.
                    connectionLimiter.Release();
                });
            }
        }

        private int FindFirstDelimiter(Span<byte> bufferSpan, Span<byte> delimiterSpan)
        {
            for (int i = 0; i <= bufferSpan.Length - delimiterSpan.Length; i++)
            {
                if (bufferSpan.Slice(i, delimiterSpan.Length).SequenceEqual(delimiterSpan))
                {
                    return i;
                }
            }
            return -1;
        }

        private async Task HandleClientConnectedAsync(TerminalTcpRouterContext tcpContext, TcpClient client, string clientId)
        {
            using (NetworkStream stream = client.GetStream())
            {
                byte[] buffer = new byte[4096];
                MemoryStream messageBuffer = new();
                bool delimetersEnabled = options.Router.EnableRemoteDelimiters.GetValueOrDefault();

                while (true)
                {
                    // Release the current thread to avoid busy-wait
                    await Task.Yield();

                    if (tcpContext.StartContext.TerminalCancellationToken.IsCancellationRequested)
                    {
                        logger.LogDebug("Client request is canceled. client_id={0}", clientId);
                        break;
                    }

                    // The stream is closed, the client is disconnected.
                    if (!client.Connected)
                    {
                        logger.LogDebug("Client is disconnected. client_id={0}", clientId);
                        break;
                    }

                    // Ensure we can read from the stream
                    if (!stream.CanRead)
                    {
                        throw new TerminalException(TerminalErrors.ServerError, "The client stream is not readable. client_id={0}", clientId);
                    }

                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, tcpContext.StartContext.TerminalCancellationToken);
                    if (bytesRead == 0)
                    {
                        logger.LogDebug("Client stream is empty. client_id={0}", clientId);
                        break;
                    }

                    // Process messages based on whether delimiters are enabled
                    if (delimetersEnabled)
                    {
                        // Write to buffer and process complete messages
                        messageBuffer.Write(buffer, 0, bytesRead);
                        ProcessCompleteMessages(messageBuffer, client.Client.RemoteEndPoint, clientId);
                    }
                    else
                    {
                        // Directly enqueue messages without processing delimiters. This assumes that the entire buffer
                        // is a single valid command
                        string message = textHandler.Encoding.GetString(buffer, 0, bytesRead);
                        commandQueue?.Enqueue(message, client.Client.RemoteEndPoint, clientId);
                    }
                }
            }
        }

        private async Task HandleClientExceptionAsync(Exception ex, string clientId)
        {
            // Ensure error message ends with a period.
            string message = ex.Message;
            if (!ex.Message.EndsWith("."))
            {
                message += '.';
            }

            if (ex.InnerException != null)
            {
                logger.LogError("{0} client_id={1} info={2}", message, clientId, ex.InnerException.Message);
            }
            else
            {
                logger.LogError("{0} client_id={1}", message, clientId);
            }

            await exceptionHandler.HandleExceptionAsync(new TerminalExceptionHandlerContext(ex, null));
        }

        private void ProcessCompleteMessages(MemoryStream messageBuffer, EndPoint clientEndpoint, string clientId)
        {
            byte[] delimiterBytes = textHandler.Encoding.GetBytes(options.Router.RemoteMessageDelimiter);
            byte[] bufferArray = messageBuffer.ToArray(); // Convert the entire MemoryStream to an array for processing
            Span<byte> dataSpan = new Span<byte>(bufferArray); // Create a span from the array

            while (true)
            {
                int delimiterIndex = FindFirstDelimiter(dataSpan, delimiterBytes.AsSpan());
                if (delimiterIndex == -1)
                {
                    break;
                }

                // Convert the relevant slice of the data span to an array for string conversion
                byte[] messageBytes = dataSpan.Slice(0, delimiterIndex + delimiterBytes.Length).ToArray();
                string message = textHandler.Encoding.GetString(messageBytes);
                commandQueue?.Enqueue(message, clientEndpoint, clientId);

                // Move the dataSpan forward past the processed message
                dataSpan = dataSpan.Slice(delimiterIndex + delimiterBytes.Length);
            }

            // Reset the messageBuffer to only contain any unprocessed data
            messageBuffer.SetLength(0);
            if (dataSpan.Length > 0)
            {
                messageBuffer.Write(dataSpan.ToArray(), 0, dataSpan.Length); // Write remaining unprocessed data back into the buffer
            }
        }

        private readonly ICommandRouter commandRouter;
        private readonly ITerminalExceptionHandler exceptionHandler;
        private readonly ILogger<TerminalTcpRouter> logger;
        private readonly TerminalOptions options;
        private readonly ITerminalTextHandler textHandler;
        private TcpListener? _server;
        private TerminalRemoteMessageQueue? commandQueue;
        private SemaphoreSlim? connectionLimiter;  // Semaphore to limit concurrent connections
    }
}
