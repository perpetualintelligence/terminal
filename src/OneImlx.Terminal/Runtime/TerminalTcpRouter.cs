/*
    Copyright (c) 2024 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Configuration.Options;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalRouter{TContext}"/> for TCP client-server communication.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class implements the <see cref="ITerminalRouter{TContext}"/> interface and is responsible for handling TCP
    /// client-server communication. It runs a terminal as a TCP server on the specified IP endpoint and waits for
    /// incoming client connections. The server can be gracefully stopped by canceling the provided cancellation token
    /// in the context.
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
            commandQueue = new TerminalCommandQueue(commandRouter, exceptionHandler, options, context, logger);
            commandCollection = new ConcurrentDictionary<int, ConcurrentQueue<string>>();
            clientConnections = new ConcurrentDictionary<Task, int>();
            connectionLimiter = new SemaphoreSlim(options.Router.MaxRemoteClients);
            clientTasks = new ConcurrentDictionary<int, Task>();

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

                // If we are here then that means a cancellation is requested, gracefully stop the client connections.
                Task routerTimeout = Task.Delay(options.Router.Timeout);
                var completedTask = await Task.WhenAny(Task.WhenAll(clientConnections.Keys), routerTimeout);
                if (completedTask == routerTimeout)
                {
                    logger.LogError("Client connections failed to complete. timeout={0}", options.Router.Timeout);
                }
                else
                {
                    logger.LogDebug("Client connections completed.");
                }
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

        /// <summary>
        /// Accepts a client connection asynchronously and handles the connection.
        /// </summary>
        /// <param name="context">The routing context.</param>
        /// <param name="taskIdx">The task index accepting a client connection.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method is responsible for accepting a single client connection asynchronously. It sets up a TCP client
        /// connection and delegates the connection handling to the HandleClientConnected method.
        /// </remarks>
        private async Task AcceptClientAsync(TerminalTcpRouterContext context, int taskIdx)
        {
            try
            {
                // Ensure that AcceptTcpClientAsync is canceled when the cancellation token is signaled.
                logger.LogDebug("Waiting for client to connect on task {0}", taskIdx);
                Task<TcpClient> tcpClientTask = _server!.AcceptTcpClientAsync();
                await Task.WhenAny(tcpClientTask, Task.Delay(Timeout.Infinite, context.StartContext.TerminalCancellationToken));
                context.StartContext.TerminalCancellationToken.ThrowIfCancellationRequested();

                // Setup the context and handle the client connection
                using TcpClient tcpClient = tcpClientTask.Result;
                logger.LogDebug("Client connected on task {0}", taskIdx);

                //await HandleClientConnectedAsync(context, tcpClient);
                logger.LogDebug("Client connection handled on task {0}", taskIdx);
            }
            catch (Exception ex)
            {
                await HandleTaskIndexedExceptionAsync(ex, taskIdx);
            }
        }

        private async Task AcceptClientsUntilCanceledAsync(TerminalTcpRouterContext context)
        {
            while (!context.StartContext.TerminalCancellationToken.IsCancellationRequested)
            {
                await connectionLimiter.WaitAsync(context.StartContext.TerminalCancellationToken);
                _ = Task.Run(async () =>
                {
                    try
                    {
                        logger.LogDebug("Waiting for client to connect...");
                        TcpClient client = await _server.AcceptTcpClientAsync();

                        int clientKey = client.Client.RemoteEndPoint.GetHashCode();
                        logger.LogDebug("Client connected on task {0}", clientKey);

                        var clientTask = HandleClientConnectedAsync(context, client, clientKey);
                        logger.LogDebug("Client connection handled on task {0}", clientKey);

                        clientTasks[clientKey] = clientTask;
                        await clientTask;
                    }
                    catch (OperationCanceledException)
                    {
                        // Release the semaphore on cancellation
                        connectionLimiter.Release();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Error accepting client: {ex.Message}");

                        // Ensure semaphore is released even on error
                        connectionLimiter.Release();
                    }
                });
            }
        }

        /// <summary>
        /// Handles the communication with a connected TCP client asynchronously.
        /// </summary>
        /// <param name="tcpContext">
        /// The <see cref="TerminalTcpRouterContext"/> representing the TCP client and server setup.
        /// </param>
        /// <param name="client">The connected client.</param>
        /// <param name="taskIdx">The task index executing the client handling.</param>
        /// <remarks>
        /// This method is executed asynchronously for each connected TCP client. It reads data from the client's
        /// network stream, processes the received data, and manages communication asynchronously.
        /// </remarks>
        private async Task HandleClientConnectedAsync(TerminalTcpRouterContext tcpContext, TcpClient client, int taskIdx)
        {
            // Check if the client is connected
            if (!client.Connected)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The client is not connected. task={0}", taskIdx);
            }

            using (NetworkStream stream = client.GetStream())
            {
                // The buffer size is the max command string length plus the delimiter length
                int bufferSize = options.Router.MaxRemoteMessageLength + options.Router.RemoteMessageDelimiter.Length;
                byte[] buffer = new byte[bufferSize];
                StringBuilder receivedData = new();

                // Read data from the client stream asynchronously until the client is disconnected or the cancellation
                // token is triggered
                while (true)
                {
                    // Release the current thread to avoid busy-wait
                    await Task.Yield();

                    if (tcpContext.StartContext.TerminalCancellationToken.IsCancellationRequested)
                    {
                        logger.LogDebug("Client request is canceled. task={0}", taskIdx);
                        break;
                    }

                    // The stream is closed, the client is disconnected.
                    if (!client.Connected)
                    {
                        logger.LogDebug("Client is disconnected. task={0}", taskIdx);
                        break;
                    }

                    // Ensure we can read from the stream
                    if (!stream.CanRead)
                    {
                        throw new TerminalException(TerminalErrors.ServerError, "The TCP client stream is not readable. task={0}", taskIdx);
                    }

                    // Read data from the stream asynchronously
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, tcpContext.StartContext.TerminalCancellationToken);
                    if (bytesRead == 0)
                    {
                        // The remote stream is closed, the client is disconnected or we have read all the data.
                        logger.LogDebug("Client stream is empty. client={0} server={1} task={2}", tcpContext.IPEndPoint.Address, _server!.LocalEndpoint, taskIdx);
                        break;
                    }

                    // Append the received data to the StringBuilder
                    commandQueue?.Enqueue(textHandler.Encoding.GetString(buffer, 0, bytesRead), client.Client.RemoteEndPoint);
                }
            }
        }

        private async Task HandleTaskIndexedExceptionAsync(Exception ex, int taskIdx)
        {
            // Ensure error message ends with a period.
            string message = ex.Message;
            if (!ex.Message.EndsWith("."))
            {
                message += '.';
            }

            if (ex.InnerException != null)
            {
                logger.LogError($"{message} task={0} info={1}", taskIdx, ex.InnerException.Message);
            }
            else
            {
                logger.LogError($"{message} task={0}", taskIdx);
            }

            await exceptionHandler.HandleExceptionAsync(new TerminalExceptionHandlerContext(ex, null));
        }

        private readonly ICommandRouter commandRouter;
        private readonly ITerminalExceptionHandler exceptionHandler;
        private readonly ILogger<TerminalTcpRouter> logger;
        private readonly TerminalOptions options;
        private readonly ITerminalTextHandler textHandler;
        private TcpListener? _server;
        private ConcurrentDictionary<Task, int>? clientConnections;
        private ConcurrentDictionary<int, Task> clientTasks;
        private ConcurrentDictionary<int, ConcurrentQueue<string>>? commandCollection;
        private TerminalCommandQueue? commandQueue;
        private SemaphoreSlim connectionLimiter;  // Semaphore to limit concurrent connections
    }
}
