/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Hosting;
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
    /// If an exception is encountered while handling client connections, the method will stop and log the exception.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "DI fields.")]
    public class TerminalTcpRouting : ITerminalRouting<TerminalTcpRoutingContext, TerminalTcpRoutingResult>
    {
        private readonly IHostApplicationLifetime applicationLifetime;
        private readonly ICommandRouter commandRouter;
        private readonly IExceptionHandler exceptionHandler;
        private readonly IErrorHandler errorHandler;
        private readonly TerminalOptions options;
        private readonly ITextHandler textHandler;
        private readonly ILogger<TerminalTcpRouting> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalTcpRouting"/> class.
        /// </summary>
        /// <param name="applicationLifetime">The host application lifetime instance.</param>
        /// <param name="commandRouter">The command router.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <param name="errorHandler">The error handler.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="logger">The logger.</param>
        /// <remarks>
        /// This constructor creates a new instance of the <see cref="TerminalTcpRouting"/> class.
        /// It takes several dependencies that are required for handling TCP client-server communication.
        /// </remarks>
        public TerminalTcpRouting(
            IHostApplicationLifetime applicationLifetime,
            ICommandRouter commandRouter,
            IExceptionHandler exceptionHandler,
            IErrorHandler errorHandler,
            TerminalOptions options,
            ITextHandler textHandler,
            ILogger<TerminalTcpRouting> logger)
        {
            this.applicationLifetime = applicationLifetime;
            this.commandRouter = commandRouter;
            this.exceptionHandler = exceptionHandler;
            this.errorHandler = errorHandler;
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
        /// <para>This method starts a TCP server on the specified IP endpoint and waits for incoming client connections.
        /// It handles the client connections asynchronously by creating a task for each incoming connection.
        /// The server can be gracefully stopped by canceling the provided cancellation token in the context.
        /// The method will also stop if an exception is encountered while handling client connections.
        /// </para>
        /// <para>Exceptions:</para>
        /// <para><see cref="ErrorException"/>: Thrown when the requested start mode is not valid for console routing.</para>
        /// <para><see cref="ErrorException"/>: Thrown when the network IP endpoint is missing in the TCP server routing request.</para>
        /// </remarks>
        public async Task<TerminalTcpRoutingResult> RunAsync(TerminalTcpRoutingContext context)
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

                // Wait for the cancellation token or the AcceptClientConnectionsAsync method to complete. A cancellation token stops the entire routing service.
                await Task.WhenAny(AcceptClientConnectionsAsync(server, context), Task.Delay(Timeout.Infinite, context.StartContext.CancellationToken));
            }
            catch (OperationCanceledException cex)
            {
                // The default exception handler will log the exception and return a generic error message to the client.
                logger.LogDebug("Cancellation requested. endpoint={0} timestamp_utc={1}", context.IPEndPoint, DateTimeOffset.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss.fff"));
                await exceptionHandler.HandleAsync(new ExceptionHandlerContext(cex, null));
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
        /// <param name="server">The TCP listener that is waiting for incoming client connections.</param>
        /// <param name="context">The routing context containing the server's configuration and start context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// <para>This method sets up a TCP listener to accept incoming client connections asynchronously. It waits for the specified
        /// synchronization delay before accepting connections. For each accepted client connection, it creates a separate background
        /// task using <see cref="Task.Run(Action)"/> to handle the connection asynchronously.
        /// </para>
        /// <para>The method attempts to accept the configured maximum number of client connections concurrently. If any exception occurs
        /// while accepting a client connection, it will be caught and logged, and the method will proceed with accepting other connections.
        /// </para>
        /// <para>The try-catch block within this method is used to handle exceptions that may occur during the process of accepting client connections.
        /// Since client connections are accepted concurrently in separate background tasks, individual client connections might encounter
        /// exceptions or fail to connect. The try-catch block ensures that any exception encountered during the acceptance process does not
        /// disrupt the overall server operation or the acceptance of other client connections.
        /// </para>
        /// <para>By logging the exception and continuing with accepting other connections, the method ensures that the server remains robust and
        /// can handle potential issues gracefully. Any exceptions logged here should be further investigated to identify and resolve the root cause.
        /// </para>
        /// <para>The method ensures that the server continues to accept new client connections and run its core logic, even if individual
        /// client connections encounter exceptions or fail to connect.
        /// </para>
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

                logger.LogDebug("Waiting for client to connect on task {0}", localIdx);
                clientConnections.Add(AcceptClientAsync(server, context, localIdx));
            }

            // Wait for all client connections to complete
            await Task.WhenAll(clientConnections);
            logger.LogWarning("All client connection tasks are exhausted.");
        }

        /// <summary>
        /// Accepts a client connection asynchronously and handles the connection.
        /// </summary>
        /// <param name="server">The TCP listener that is waiting for incoming client connections.</param>
        /// <param name="context">The routing context containing the server's configuration and start context.</param>
        /// <param name="taskIdx">The task index that is accepting a client connection.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// <para>This method is called for each accepted client connection. It sets up a new TCP client connection (<see cref="TcpClient"/>)
        /// and then calls the <see cref="HandleClientConnected"/> method to process the client's commands.
        /// </para>
        /// </remarks>
        private async Task AcceptClientAsync(TcpListener server, TerminalTcpRoutingContext context, int taskIdx)
        {
            try
            {
                using TcpClient tcpClient = await server.AcceptTcpClientAsync();
                context.Setup(server, tcpClient);
                await HandleClientConnected(context);
            }
            catch (Exception ex)
            {
                await exceptionHandler.HandleAsync(new ExceptionHandlerContext(ex, null));
            }
        }

        private async Task ProcessRawDataAsync(TerminalTcpRoutingContext tcpContext, string raw)
        {
            // Route the command request to router. Wait for the router or the timeout.
            CommandRouterContext context = new(raw, tcpContext.StartContext.CancellationToken);
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
        /// <param name="tcpContext">The TerminalTcpRoutingContext representing the TCP client and server setup.</param>
        /// <remarks>
        /// <para>This method is executed asynchronously for each connected TCP client. It first checks if the TCP client and server
        /// are set up correctly. If not, it throws an ErrorException with an appropriate error message.
        /// </para>
        /// <para>Next, the method checks if the client is connected. If the client is not connected, it throws an ErrorException
        /// indicating that the TCP client is not connected.
        /// </para>
        /// <para>If the TCP client is properly set up and connected, it reads data from the client's NetworkStream asynchronously
        /// until either the client is disconnected or the cancellation token is triggered. If the client is disconnected,
        /// the method logs a debug message and exits the loop.
        /// </para>
        /// <para>For each read operation, the received data is appended to a StringBuilder, as the data may not arrive as complete
        /// packages. The received data is then split using the configured CommandStringDelimiter to identify complete data packages.
        /// </para>
        /// <para>If no complete data package is found, the method continues reading from the stream. Once a complete data package is
        /// identified, the method processes the data package and moves to the next one until all received data is processed.
        /// </para>
        /// <para>Before processing each data package, the method checks if the data package size is over the configured limit. If the
        /// package size exceeds the limit, it throws an ErrorException indicating that the command string length is over the
        /// configured limit.
        /// </para>
        /// <para>If the last data package is not complete, the method stores the incomplete part in the StringBuilder to be processed
        /// along with the next incoming data.
        /// </para>
        /// <para>The method is designed to avoid blocking indefinitely by using the CancellationToken from the provided TerminalTcpRoutingContext's
        /// StartContext. When the CancellationToken is triggered, the method stops reading data and returns.
        /// </para>
        /// </remarks>
        private async Task HandleClientConnected(TerminalTcpRoutingContext tcpContext)
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
                while (!tcpContext.StartContext.CancellationToken.IsCancellationRequested)
                {
                    // The stream is closed, the client is disconnected.
                    if (!tcpContext.Client.Connected)
                    {
                        logger.LogDebug("Client is disconnected. client_endpoint={0} server_endpoint={1}", tcpContext.Client.Client.LocalEndPoint.ToString(), tcpContext.Server.LocalEndpoint.ToString());
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