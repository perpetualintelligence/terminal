﻿/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        /// <param name="options">The configuration options.</param>
        /// <param name="commandRouter">The command router.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <param name="terminalProcessor">The terminal processing queue.</param>
        /// <param name="logger">The logger.</param>
        /// <remarks>
        /// This constructor creates a new instance of the <see cref="TerminalTcpRouter"/> class. It takes several
        /// dependencies that are required for handling TCP client-server communication.
        /// </remarks>
        public TerminalTcpRouter(
            IOptions<TerminalOptions> options,
            ICommandRouter commandRouter,
            ITerminalTextHandler textHandler,
            ITerminalExceptionHandler exceptionHandler,
            ITerminalProcessor terminalProcessor,
            ILogger<TerminalTcpRouter> logger)
        {
            this.commandRouter = commandRouter;
            this.exceptionHandler = exceptionHandler;
            this.terminalProcessor = terminalProcessor;
            this.options = options;
            this.textHandler = textHandler;
            this.logger = logger;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="TerminalTcpRouter"/> is running.
        /// </summary>
        public bool IsRunning { get; protected set; }

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

            // Start the TCP server
            _server = new(context.IPEndPoint);
            Task backgroundProcessingTask = Task.CompletedTask;
            try
            {
                // Start the TCP server
                _server.Start();
                logger.LogDebug("Terminal TCP router started. endpoint={0}", context.IPEndPoint);
                IsRunning = true;

                // Start processing the command queue immediately in the background. The code starts the background task
                // for processing commands immediately and does not wait for it to complete. The _ = discards the
                // returned task since we don't need to await it in this context. It effectively runs in the background,
                // processing commands as they are enqueued.
                terminalProcessor.StartProcessing(context);

                // Blocking call to accept client connections until the cancellation token is requested. We have
                // initialized the requested client connections. Now we wait for all the client connections to complete
                // until the cancellation requested. Each time a a client task complete a new task is created to accept
                // a new client connection.
                await AcceptClientsUntilCanceledAsync(context);

                // Throw if canceled
                context.StartContext.TerminalCancellationToken.ThrowIfCancellationRequested();
            }
            catch (Exception ex)
            {
                // The default exception handler will log the exception and return a generic error message to the client.
                await exceptionHandler.HandleExceptionAsync(new TerminalExceptionHandlerContext(ex, null));
            }
            finally
            {
                // Stop the TCP server
                _server.Stop();

                // Stop and await for the background command processing to complete
                await terminalProcessor.StopProcessingAsync(options.Value.Router.Timeout);

                // Ensure the command queue is stopped
                logger.LogDebug("Terminal TCP router stopped. endpoint={0}", context.IPEndPoint);
                IsRunning = false;
            }
        }

        private async Task AcceptClientAsync(TerminalTcpRouterContext context, CancellationToken terminalCancellationToken, string clientId)
        {
            try
            {
                // Wait for the client to connect, take into account the cancellation token for .NET Standard 2.0
                var acceptTask = _server!.AcceptTcpClientAsync();
                var cancellationTask = Task.Delay(Timeout.Infinite, terminalCancellationToken);
                var completedTask = await Task.WhenAny(acceptTask, cancellationTask);
                if (completedTask == cancellationTask)
                {
                    return;
                }

                // Wait for the client connection acceptance to complete
                using (TcpClient acceptClient = await acceptTask)
                {
                    logger.LogDebug("Client connected. client={0} remote={1}", clientId, acceptClient.Client.RemoteEndPoint?.ToString());

                    // Handle the client connection
                    await HandleClientConnectedAsync(context, acceptClient, clientId);
                }
            }
            catch (Exception ex)
            {
                await exceptionHandler.HandleExceptionAsync(new TerminalExceptionHandlerContext(ex, null));
            }
        }

        private async Task AcceptClientsUntilCanceledAsync(TerminalTcpRouterContext context)
        {
            CancellationToken terminalCancellationToken = context.StartContext.TerminalCancellationToken;

            // Ensure the server is initialized
            if (_server == null)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The TCP server is not initialized.");
            }

            // The server is running and accepting client connections. We will wait for incoming client connections
            ConcurrentDictionary<string, Task> clientTasks = [];
            while (true)
            {
                // Ensure we have not reached the maximum number of remote clients
                if (clientTasks.Count >= options.Value.Router.MaxRemoteClients)
                {
                    // Wait for any client task to complete before accepting a new one
                    Task completedTask = await Task.WhenAny(clientTasks.Values);
                    var completedKvp = clientTasks.First(e => e.Value.Equals(completedTask));
                    clientTasks.TryRemove(completedKvp.Key, out Task? removedTask);
                }

                // If cancellation is requested, break the loop before we setup a new client accept task
                if (terminalCancellationToken.IsCancellationRequested)
                {
                    logger.LogDebug("Terminal TCP router canceled.");
                    break;
                }

                // Initialize a new client connection. Note: we are not awaiting the task here to allow multiple client connections.
                string clientId = terminalProcessor.NewUniqueId("client");
                Task clientTask = AcceptClientAsync(context, terminalCancellationToken, clientId);
                clientTasks.TryAdd(clientId, clientTask);
            }

            // We are here that means the cancellation is requested. Wait for all remaining client tasks to complete.
            await Task.WhenAll(clientTasks.Values);
        }

        private async Task HandleClientConnectedAsync(TerminalTcpRouterContext tcpContext, TcpClient client, string clientId)
        {
            while (true)
            {
                if (tcpContext.StartContext.TerminalCancellationToken.IsCancellationRequested)
                {
                    logger.LogDebug("Client request is canceled. client={0}", clientId);
                    break;
                }

                // The stream is closed, the client is disconnected.
                if (!client.Connected)
                {
                    logger.LogDebug("Client is disconnected. client={0}", clientId);
                    break;
                }

                // Add to the terminal processor
                byte[] bytesRead = new byte[2048];
                int result = await client.GetStream().ReadAsync(bytesRead, 0, 2048);
                if (result == 0)
                {
                    logger.LogDebug("Client is disconnected. client={0}", clientId);
                    break;
                }
                else
                {
                    await terminalProcessor.AddRequestAsync(textHandler.Encoding.GetString(bytesRead, 0, result), clientId, client.Client.RemoteEndPoint?.ToString());
                }
            }
        }

        private readonly ICommandRouter commandRouter;
        private readonly ITerminalExceptionHandler exceptionHandler;
        private readonly ILogger<TerminalTcpRouter> logger;
        private readonly IOptions<TerminalOptions> options;
        private readonly ITerminalProcessor terminalProcessor;
        private readonly ITerminalTextHandler textHandler;
        private TcpListener? _server;
    }
}
