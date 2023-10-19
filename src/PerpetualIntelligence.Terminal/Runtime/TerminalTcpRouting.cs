/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Commands.Routers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalRouting{TContext}"/> for TCP client-server communication.
    /// </summary>
    /// <remarks>
    /// <para>This class implements the <see cref="ITerminalRouting{TContext}"/> interface and is responsible for handling
    /// TCP client-server communication. It runs a TCP server on the specified IP endpoint and waits for incoming client connections.
    /// The server can be gracefully stopped by canceling the provided cancellation token in the context.
    /// </para>
    /// </remarks>
    public class TerminalTcpRouting : ITerminalRouting<TerminalTcpRoutingContext>
    {
        private readonly ICommandRouter commandRouter;
        private readonly IExceptionHandler exceptionHandler;
        private readonly TerminalOptions options;
        private readonly ITextHandler textHandler;
        private readonly ILogger<TerminalTcpRouting> logger;
        private ConcurrentDictionary<int, ConcurrentQueue<string>>? commandCollection;
        private ConcurrentDictionary<Task, int>? clientConnections;
        private TcpListener? _server;

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalTcpRouting"/> class.
        /// </summary>
        /// <param name="commandRouter">The command router.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="logger">The logger.</param>
        /// <remarks>
        /// This constructor creates a new instance of the <see cref="TerminalTcpRouting"/> class.
        /// It takes several dependencies that are required for handling TCP client-server communication.
        /// </remarks>
        public TerminalTcpRouting(
            ICommandRouter commandRouter,
            IExceptionHandler exceptionHandler,
            TerminalOptions options,
            ITextHandler textHandler,
            ILogger<TerminalTcpRouting> logger)
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
        /// This method starts a TCP server on the specified IP endpoint and waits for incoming client connections.
        /// It handles the client connections asynchronously by creating a task for each incoming connection.
        /// The server can be gracefully stopped by canceling the provided cancellation token in the context.
        /// The method will also stop if an exception is encountered while handling client connections.
        /// </remarks>
        public virtual async Task RunAsync(TerminalTcpRoutingContext context)
        {
            // Reset the command queue
            commandCollection = new ConcurrentDictionary<int, ConcurrentQueue<string>>();
            clientConnections = new ConcurrentDictionary<Task, int>();

            // Ensure we have supported start context
            if (context.StartContext.StartInformation.StartMode != TerminalStartMode.Tcp)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The requested start mode is not valid for console routing. start_mode={0}", context.StartContext.StartInformation.StartMode);
            }

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

                // Initialize the client connections
                // Setup tasks to accept client connections
                logger.LogDebug("Initializing client connections. count={0}", options.Router.MaxRemoteClients);
                for (int idx = 0; idx < options.Router.MaxRemoteClients; ++idx)
                {
                    // Create a local copy of the index for the task
                    int localIdx = idx + 1;
                    clientConnections.TryAdd(AcceptClientAsync(context, localIdx), localIdx);
                }

                // We have initialized the requested client connections. Now we wait for all the client connections to complete until
                // the cancellation requested. Each time a a client task complete a new task is created to accept a new client connection.
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
                    logger.LogDebug("Client connections are complete.");
                }
            }
            catch (Exception ex)
            {
                // The default exception handler will log the exception and return a generic error message to the client.
                await exceptionHandler.HandleExceptionAsync(new ExceptionHandlerContext(ex, null));
            }
            finally
            {
                _server.Stop(); // Stop the TCP server
                logger.LogDebug("Terminal TCP routing stopped. endpoint={0}", context.IPEndPoint);
            }
        }

        private async Task AcceptClientsUntilCanceledAsync(TerminalTcpRoutingContext context)
        {
            if (commandCollection is null || clientConnections is null)
            {
                return;
            }

            // Keep accepting new clients till a cancellation token is received.
            CancellationToken cancellationToken = context.StartContext.CancellationToken;
            while (!cancellationToken.IsCancellationRequested)
            {
                int completedIndex = -1;
                try
                {
                    // Process the commands from the completed client connection
                    Task completedClient = await Task.WhenAny(clientConnections.Keys);
                    completedIndex = clientConnections[completedClient];
                    if (commandCollection.TryRemove(completedIndex, out ConcurrentQueue<string> commands))
                    {
                        foreach (string raw in commands)
                        {
                            await ProcessRawCommandsAsync(context, raw, completedIndex);
                        }
                    }

                    // Remove the completed client connection and add a new one
                    logger.LogWarning("Client connection is exhausted. task={0}", clientConnections[completedClient]);
                    clientConnections.TryRemove(completedClient, out int value);
                    logger.LogDebug("Initializing a new client connection. task={0}", completedIndex);
                    clientConnections.TryAdd(AcceptClientAsync(context, completedIndex), completedIndex);
                }
                catch (Exception ex)
                {
                    await HandleTaskIndexedExceptionAsync(ex, completedIndex);
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

            await exceptionHandler.HandleExceptionAsync(new ExceptionHandlerContext(ex, null));
        }

        /// <summary>
        /// Accepts a client connection asynchronously and handles the connection.
        /// </summary>
        /// <param name="context">The routing context.</param>
        /// <param name="taskIdx">The task index accepting a client connection.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method is responsible for accepting a single client connection asynchronously.
        /// It sets up a TCP client connection and delegates the connection handling to the HandleClientConnected method.
        /// </remarks>
        private async Task AcceptClientAsync(TerminalTcpRoutingContext context, int taskIdx)
        {
            try
            {
                // Ensure that AcceptTcpClientAsync is canceled when the cancellation token is signaled.
                logger.LogDebug("Waiting for client to connect on task {0}", taskIdx);
                Task<TcpClient> tcpClientTask = _server!.AcceptTcpClientAsync();
                await Task.WhenAny(tcpClientTask, Task.Delay(Timeout.Infinite, context.StartContext.CancellationToken));
                context.StartContext.CancellationToken.ThrowIfCancellationRequested();

                // Setup the context and handle the client connection
                using TcpClient tcpClient = tcpClientTask.Result;
                logger.LogDebug("Client connected on task {0}", taskIdx);

                await HandleClientConnectedAsync(context, tcpClient, taskIdx);
                logger.LogDebug("Client connection handled on task {0}", taskIdx);
            }
            catch (Exception ex)
            {
                await HandleTaskIndexedExceptionAsync(ex, taskIdx);
            }
        }

        private async Task ProcessRawCommandsAsync(TerminalTcpRoutingContext tcpContext, string raw, int taskIdx)
        {
            logger.LogDebug("Routing the command. raw={0} task={1}", raw, taskIdx);

            // Route the command request to router. Wait for the router or the timeout.
            CommandRouterContext context = new(raw, tcpContext);
            Task<CommandRouterResult> routeTask = commandRouter.RouteCommandAsync(context);
            if (await Task.WhenAny(routeTask, Task.Delay(options.Router.Timeout, tcpContext.StartContext.CancellationToken)) == routeTask)
            {
                // Task completed within timeout.
                // Consider that the task may have faulted or been canceled.
                // We re-await the task so that any exceptions/cancellation is re-thrown.
                await routeTask;
            }
            else
            {
                throw new TimeoutException($"The command router timed out in {options.Router.Timeout} milliseconds.");
            }
        }

        /// <summary>
        /// Handles the communication with a connected TCP client asynchronously.
        /// </summary>
        /// <param name="tcpContext">The <see cref="TerminalTcpRoutingContext"/> representing the TCP client and server setup.</param>
        /// <param name="client">The connected client.</param>
        /// <param name="taskIdx">The task index executing the client handling.</param>
        /// <remarks>
        /// This method is executed asynchronously for each connected TCP client.
        /// It reads data from the client's network stream, processes the received data, and manages communication asynchronously.
        /// </remarks>
        private async Task HandleClientConnectedAsync(TerminalTcpRoutingContext tcpContext, TcpClient client, int taskIdx)
        {
            // Check if the client is connected
            if (!client.Connected)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, $"The client is not connected. task={0}", taskIdx);
            }

            using (NetworkStream stream = client.GetStream())
            {
                // The buffer size is the max command string length plus the delimiter length
                int bufferSize = options.Router.MaxMessageLength + options.Router.MessageDelimiter.Length;
                byte[] buffer = new byte[bufferSize];
                StringBuilder receivedData = new();

                // Read data from the client stream asynchronously until the client is disconnected or the cancellation token is triggered
                while (true)
                {
                    // Release the current thread to avoid busy-wait
                    await Task.Yield();

                    if (tcpContext.StartContext.CancellationToken.IsCancellationRequested)
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
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, tcpContext.StartContext.CancellationToken);
                    if (bytesRead == 0)
                    {
                        // The remote stream is closed, the client is disconnected or we have read all the data.
                        logger.LogDebug("Client stream is empty. client={0} server={1} task={2}", tcpContext.IPEndPoint.Address, _server!.LocalEndpoint, taskIdx);
                        break;
                    }

                    // Append the received data to the StringBuilder
                    receivedData.Append(textHandler.Encoding.GetString(buffer, 0, bytesRead));
                }

                // The raw received data is the data received so far. We need to sync it with the previous iteration data
                // that was not processed.
                string rawCommandString = receivedData.ToString();
                if (string.IsNullOrWhiteSpace(rawCommandString))
                {
                    throw new TerminalException(TerminalErrors.ServerError, "The command string cannot be empty. task={0}", taskIdx);
                }

                // This may be multiple commands
                string[] splitCmdString = rawCommandString.Split(new[] { options.Router.MessageDelimiter }, StringSplitOptions.RemoveEmptyEntries);
                for (int idx = 0; idx < splitCmdString.Length; ++idx)
                {
                    string raw = splitCmdString[idx];
                    if (raw.Length > options.Router.MaxMessageLength)
                    {
                        throw new TerminalException(TerminalErrors.InvalidRequest, "The command string length is over the configured limit. max_length={0} task={1}", options.Router.MaxMessageLength, taskIdx);
                    }

                    // Process the completed data package if it hasn't been processed before
                    bool found = commandCollection!.TryGetValue(taskIdx, out ConcurrentQueue<string> values);
                    if (!found)
                    {
                        values = new ConcurrentQueue<string>();
                        commandCollection.TryAdd(taskIdx, values);
                    }
                    values.Enqueue(raw);
                }
            }
        }
    }
}