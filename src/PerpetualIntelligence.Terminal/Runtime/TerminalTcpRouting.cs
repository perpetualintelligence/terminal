/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Commands.Routers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalRouting{TContext, TResult}"/> for TCP client-server communication.
    /// </summary>
    /// <remarks>
    /// <para>This class implements the <see cref="ITerminalRouting{TContext, TResult}"/> interface and is responsible for handling
    /// TCP client-server communication. It runs a TCP server on the specified IP endpoint and waits for incoming client connections.
    /// The server can be gracefully stopped by canceling the provided cancellation token in the context.
    /// </para>
    /// </remarks>
    public class TerminalTcpRouting : ITerminalRouting<TerminalTcpRoutingContext, TerminalTcpRoutingResult>
    {
        private readonly ICommandRouter commandRouter;
        private readonly IExceptionHandler exceptionHandler;
        private readonly TerminalOptions options;
        private readonly ITextHandler textHandler;
        private readonly ILogger<TerminalTcpRouting> logger;

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
        public virtual async Task<TerminalTcpRoutingResult> RunAsync(TerminalTcpRoutingContext context)
        {
            // Ensure we have supported start context
            if (context.StartContext.StartInformation.StartMode != TerminalStartMode.Tcp)
            {
                throw new ErrorException(TerminalErrors.InvalidConfiguration, "The requested start mode is not valid for console routing. start_mode={0}", context.StartContext.StartInformation.StartMode);
            }

            if (context.IPEndPoint == null)
            {
                throw new ErrorException(TerminalErrors.InvalidConfiguration, "The network IP endpoint is missing in the TCP server routing request.");
            }

            TcpListener server = new(context.IPEndPoint);

            try
            {
                // Start the TCP server
                server.Start();
                logger.LogDebug("TCP Server started. endpoint={0} timestamp_utc={1}", context.IPEndPoint, DateTimeOffset.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss.fff"));

                // Indefinitely wait for incoming clients till a cancellation token.
                while (true)
                {
                    // Break if cancellation is requested
                    if (context.StartContext.CancellationToken.IsCancellationRequested)
                    {
                        logger.LogDebug("TCP Server is canceled. endpoint={0} timestamp_utc={1}", context.IPEndPoint, DateTimeOffset.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss.fff"));
                        break;
                    }

                    await AcceptClientConnectionsAsync(server, context);
                }
            }
            catch (Exception ex)
            {
                // The default exception handler will log the exception and return a generic error message to the client.
                await exceptionHandler.HandleAsync(new ExceptionHandlerContext(ex, null));
            }
            finally
            {
                server.Stop(); // Stop the TCP server
                logger.LogDebug("Terminal TCP routing stopped. endpoint={0} timestamp_utc={1}", context.IPEndPoint, DateTimeOffset.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss.fff"));
            }

            // Return the default result (empty)
            return new TerminalTcpRoutingResult();
        }

        /// <summary>
        /// Accepts multiple client connections asynchronously and handles each connection in a separate background task.
        /// </summary>
        /// <param name="server">The TCP listener waiting for incoming client connections.</param>
        /// <param name="context">The routing context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method sets up a TCP listener to accept incoming client connections asynchronously.
        /// It handles the accepted connections concurrently by creating separate background tasks.
        /// Exceptions encountered during acceptance are logged, and the method continues accepting other connections.
        /// </remarks>
        private async Task AcceptClientConnectionsAsync(TcpListener server, TerminalTcpRoutingContext context)
        {
            // Setup tasks to accept client connections
            logger.LogDebug("Initializing {0} client connections.", options.Router.RemoteMaxClients);
            List<Task> clientConnections = new();
            for (int idx = 0; idx < options.Router.RemoteMaxClients; ++idx)
            {
                // Create a local copy of the index for the task
                int localIdx = idx + 1;
                clientConnections.Add(AcceptClientAsync(server, context, localIdx));
            }

            // Wait for all client connections to complete
            await Task.WhenAll(clientConnections);
            logger.LogWarning("All {0} client connection tasks are exhausted.", options.Router.RemoteMaxClients);
        }

        /// <summary>
        /// Accepts a client connection asynchronously and handles the connection.
        /// </summary>
        /// <param name="server">The TCP listener waiting for incoming client connections.</param>
        /// <param name="context">The routing context.</param>
        /// <param name="taskIdx">The task index accepting a client connection.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method is responsible for accepting a single client connection asynchronously.
        /// It sets up a TCP client connection and delegates the connection handling to the HandleClientConnected method.
        /// </remarks>
        private async Task AcceptClientAsync(TcpListener server, TerminalTcpRoutingContext context, int taskIdx)
        {
            try
            {
                // Ensure that AcceptTcpClientAsync is canceled when the cancellation token is signaled.
                logger.LogDebug("Waiting for client to connect on task {0}", taskIdx);
                Task<TcpClient> tcpClientTask = server.AcceptTcpClientAsync();
                await Task.WhenAny(tcpClientTask, Task.Delay(Timeout.Infinite, context.StartContext.CancellationToken));
                context.StartContext.CancellationToken.ThrowIfCancellationRequested();

                // Setup the context and handle the client connection
                using TcpClient tcpClient = tcpClientTask.Result;
                context.Setup(server, tcpClient);
                logger.LogDebug("Client connected on task {0}", taskIdx);

                await HandleClientConnected(context, taskIdx);
                logger.LogDebug("Client connection handled on task {0}", taskIdx);
            }
            catch (Exception ex)
            {
                await exceptionHandler.HandleAsync(new ExceptionHandlerContext(ex, null));
            }
        }

        /// <summary>
        /// Processes the raw data received from the client asynchronously.
        /// </summary>
        /// <param name="tcpContext">The <see cref="TerminalTcpRoutingContext"/> representing the TCP client and server setup.</param>
        /// <param name="raw">The raw data received from the client.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method routes the command request to the router. It waits for the router or the timeout.
        /// The task completed within the timeout. If it faults or is canceled, the exception is re-thrown.
        /// </remarks>
        private async Task ProcessRawDataAsync(TerminalTcpRoutingContext tcpContext, string raw)
        {
            // Route the command request to router. Wait for the router or the timeout.
            CommandRouterContext context = new(raw, tcpContext);
            Task<CommandRouterResult> routeTask = commandRouter.RouteAsync(context);
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
        /// <param name="taskIdx">The task index executing the client handling.</param>
        /// <remarks>
        /// This method is executed asynchronously for each connected TCP client.
        /// It reads data from the client's network stream, processes the received data, and manages communication asynchronously.
        /// </remarks>
        private async Task HandleClientConnected(TerminalTcpRoutingContext tcpContext, int taskIdx)
        {
            // Check if the TCP client and server are set up correctly
            if (tcpContext.Server == null || tcpContext.Client == null)
            {
                throw new ErrorException(TerminalErrors.InvalidConfiguration, "The TCP routing context is not set up with a client and server.");
            }

            // Check if the client is connected
            if (!tcpContext.Client.Connected)
            {
                throw new ErrorException(TerminalErrors.InvalidConfiguration, "The TCP client is not connected.");
            }

            using (NetworkStream stream = tcpContext.Client.GetStream())
            {
                // The buffer size is the max command string length plus the delimiter length
                int bufferSize = options.Router.MaxCommandStringLength + options.Router.CommandStringDelimiter.Length;
                byte[] buffer = new byte[bufferSize];
                StringBuilder receivedData = new();

                // Read data from the client stream asynchronously until the client is disconnected or the cancellation token is triggered
                while (true)
                {
                    if (tcpContext.StartContext.CancellationToken.IsCancellationRequested)
                    {
                        logger.LogDebug("Client request is canceled. task_idx={0} timestamp_utc={1}", taskIdx, DateTimeOffset.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss.fff"));
                        break;
                    }

                    // The stream is closed, the client is disconnected.
                    if (!tcpContext.Client.Connected)
                    {
                        logger.LogDebug("Client is disconnected. task_idx={0} timestamp_utc={1}", taskIdx, DateTimeOffset.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss.fff"));
                        break;
                    }

                    // Read data from the stream asynchronously
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        continue;
                    }

                    // Append the received data to the StringBuilder
                    receivedData.Append(textHandler.Encoding.GetString(buffer, 0, bytesRead));

                    // The raw received data is the data received so far. We need to sync it with the previous iteration data
                    // that was not processed.
                    string syncedRawReceivedData = receivedData.ToString();

                    // Split the received data using the delimiter
                    bool lastMessageComplete = syncedRawReceivedData.EndsWith(options.Router.CommandStringDelimiter);
                    string[] splitPackages = syncedRawReceivedData.Split(new[] { options.Router.CommandStringDelimiter }, StringSplitOptions.RemoveEmptyEntries);

                    // If the split packages are empty then we do not have any complete message, continue and read more data.
                    if (!splitPackages.Any())
                    {
                        continue;
                    }

                    // Process all completed data packages
                    for (int idx = 0; idx < splitPackages.Length; ++idx)
                    {
                        string dp = splitPackages[idx];

                        // Check if the last data package is complete
                        bool isLastPackage = idx == splitPackages.Length - 1;

                        // Check if data package size is over the configured limit. If so, throw an exception. Also check if the last data package is complete.
                        if (!isLastPackage || (isLastPackage && lastMessageComplete))
                        {
                            if (dp.Length > options.Router.MaxCommandStringLength)
                            {
                                throw new ErrorException(TerminalErrors.InvalidRequest, "The command string length is over the configured limit. max_length={0}", options.Router.MaxCommandStringLength);
                            }

                            // Process the completed data package if it hasn't been processed before
                            await ProcessRawDataAsync(tcpContext, dp);
                        }
                    }

                    // Check if the last message was complete then we have to clear the received data to avoid processing the message again.
                    receivedData.Clear();
                    if (!lastMessageComplete)
                    {
                        receivedData.Append(splitPackages.Last());
                    }
                }
            }
        }
    }
}