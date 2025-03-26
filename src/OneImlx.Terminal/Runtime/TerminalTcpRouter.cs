/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Concurrent;
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
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <param name="terminalProcessor">The terminal processing queue.</param>
        /// <param name="logger">The logger.</param>
        /// <remarks>
        /// This constructor creates a new instance of the <see cref="TerminalTcpRouter"/> class. It takes several
        /// dependencies that are required for handling TCP client-server communication.
        /// </remarks>
        public TerminalTcpRouter(
            IOptions<TerminalOptions> options,
            ITerminalExceptionHandler exceptionHandler,
            ITerminalProcessor terminalProcessor,
            ILogger<TerminalTcpRouter> logger)
        {
            this.exceptionHandler = exceptionHandler;
            this.terminalProcessor = terminalProcessor;
            this.options = options;
            this.logger = logger;
            this.tcpClients = new ConcurrentDictionary<string, TcpClient>();
            this.tcpClientClosure = new ConcurrentDictionary<string, bool>();
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
            if (context.StartMode != TerminalStartMode.Tcp)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The requested start mode is not valid for TCP routing. start_mode={0}", context.StartMode);
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
                terminalProcessor.StartProcessing(context, background: true, responseHandler: HandleResponseAsync);

                // Blocking call to accept client connections until the cancellation token is requested. We have
                // initialized the requested client connections. Now we wait for all the client connections to complete
                // until the cancellation requested. Each time a a client task complete a new task is created to accept
                // a new client connection.
                await AcceptClientsUntilCanceledAsync(context);

                // Throw if canceled
                context.TerminalCancellationToken.ThrowIfCancellationRequested();
            }
            catch (Exception ex)
            {
                // The default exception handler will log the exception and return a generic error message to the client.
                await exceptionHandler.HandleExceptionAsync(new TerminalExceptionHandlerContext(ex, null));
            }
            finally
            {
                // Stop and await for the background command processing to complete
                await terminalProcessor.StopProcessingAsync(5000);

                // Gracefully close all clients when cancellation is requested
                foreach (var client in tcpClients.Values)
                {
                    client.Close();
                }
                tcpClients.Clear();
                tcpClientClosure.Clear();

                // Stop the TCP server
                _server.Stop();

                // Ensure the command queue is stopped
                logger.LogDebug("Terminal TCP router stopped. endpoint={0}", context.IPEndPoint);
                IsRunning = false;
            }
        }

        private async Task AcceptClientAsync(TerminalTcpRouterContext context, CancellationToken terminalCancellationToken, string clientId)
        {
            try
            {
                // Wait for the client to connect, taking into account the cancellation token
                var acceptTask = _server!.AcceptTcpClientAsync();
                var cancellationTask = Task.Delay(Timeout.Infinite, terminalCancellationToken);
                var completedTask = await Task.WhenAny(acceptTask, cancellationTask);
                if (completedTask == cancellationTask)
                {
                    return;
                }

                // Wait for the client connection acceptance to complete
                TcpClient acceptClient = await acceptTask;
                tcpClients.TryAdd(clientId, acceptClient);
                logger.LogDebug("Client connected. client={0} remote={1}", clientId, acceptClient.Client.RemoteEndPoint?.ToString());
                await HandleClientConnectedAsync(context, acceptClient, clientId);
            }
            catch (Exception ex)
            {
                await exceptionHandler.HandleExceptionAsync(new TerminalExceptionHandlerContext(ex, null));
            }
            finally
            {
                // Mark the TCP client for closure
                tcpClientClosure.TryAdd(clientId, true);
                clientSemiphore?.Release();
            }
        }

        private async Task AcceptClientsUntilCanceledAsync(TerminalTcpRouterContext context)
        {
            CancellationToken terminalCancellationToken = context.TerminalCancellationToken;

            if (_server == null)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The TCP server is not initialized.");
            }

            // Setup max client connections.
            clientSemiphore = new SemaphoreSlim(options.Value.Router.MaxClients, options.Value.Router.MaxClients);

            try
            {
                while (!terminalCancellationToken.IsCancellationRequested)
                {
                    // Wait for a slot to become available before accepting a new client
                    await clientSemiphore.WaitAsync(terminalCancellationToken);

                    // Start accepting a new client without blocking
                    string clientId = Guid.NewGuid().ToString();
                    _ = AcceptClientAsync(context, terminalCancellationToken, clientId);
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogDebug("Terminal TCP router canceled.");
            }
            catch (Exception ex)
            {
                await exceptionHandler.HandleExceptionAsync(new TerminalExceptionHandlerContext(ex, null));
            }
            finally
            {
                clientSemiphore.Dispose();
            }
        }

        private async Task HandleClientConnectedAsync(TerminalTcpRouterContext tcpContext, TcpClient client, string clientId)
        {
            while (true)
            {
                if (tcpContext.TerminalCancellationToken.IsCancellationRequested)
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
                byte[] buffer = new byte[4096];
                int bytesRead = await client.GetStream().ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    logger.LogDebug("Client is disconnected. client={0}", clientId);
                    break;
                }
                else
                {
                    // Create a memory stream to pass the buffer to the terminal processor
                    await terminalProcessor.StreamAsync(buffer, bytesRead, clientId, client.Client.RemoteEndPoint?.ToString());
                }
            }
        }

        private async Task HandleResponseAsync(TerminalInputOutput terminalIO)
        {
            TcpClient? client = null;
            string? clientId = null;

            try
            {
                // Client id is invalid
                clientId = terminalIO.SenderId ?? throw new TerminalException(TerminalErrors.ServerError, "The sender identifier is missing the response.");
                tcpClients.TryGetValue(clientId, out client);
                if (client == null)
                {
                    throw new TerminalException(TerminalErrors.ServerError, "The client id is not found in the client collection. client={0}", clientId);
                }

                byte[] responseBytes = TerminalServices.DelimitBytes(JsonSerializer.SerializeToUtf8Bytes(terminalIO), options.Value.Router.StreamDelimiter);
                await client.GetStream().WriteAsync(responseBytes, 0, responseBytes.Length);
            }
            catch (Exception ex)
            {
                await exceptionHandler.HandleExceptionAsync(new TerminalExceptionHandlerContext(ex, null));
            }
            finally
            {
                if (clientId != null)
                {
                    tcpClientClosure.TryGetValue(clientId, out bool markedForClosure);
                    if (markedForClosure)
                    {
                        if (client != null)
                        {
                            client.Close();
                            tcpClients.TryRemove(clientId, out _);
                            tcpClientClosure.TryRemove(clientId, out _);
                            logger.LogDebug("Response not handled since client is marked for closure. client={0}", clientId);
                        }
                    }
                }
            }
        }

        private readonly ITerminalExceptionHandler exceptionHandler;
        private readonly ILogger<TerminalTcpRouter> logger;
        private readonly IOptions<TerminalOptions> options;
        private readonly ConcurrentDictionary<string, bool> tcpClientClosure;
        private readonly ConcurrentDictionary<string, TcpClient> tcpClients;
        private readonly ITerminalProcessor terminalProcessor;
        private TcpListener? _server;
        private SemaphoreSlim? clientSemiphore;
    }
}
