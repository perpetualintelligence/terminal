/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Infrastructure;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Routers
{
    /// <summary>
    /// The default <see cref="IRoutingService"/> for TCP server.
    /// </summary>
    public class TcpServerRoutingService : IRoutingService
    {
        private readonly IHostApplicationLifetime applicationLifetime;
        private readonly ICommandRouter commandRouter;
        private readonly IExceptionHandler exceptionHandler;
        private readonly IErrorHandler errorHandler;
        private readonly CliOptions options;
        private readonly ITextHandler textHandler;
        private readonly ILogger<TcpServerRoutingService> logger;

        /// <summary>
        ///
        /// </summary>
        /// <param name="applicationLifetime"></param>
        /// <param name="commandRouter"></param>
        /// <param name="exceptionHandler"></param>
        /// <param name="errorHandler"></param>
        /// <param name="options"></param>
        /// <param name="textHandler"></param>
        /// <param name="logger"></param>
        public TcpServerRoutingService(IHostApplicationLifetime applicationLifetime, ICommandRouter commandRouter, IExceptionHandler exceptionHandler, IErrorHandler errorHandler, CliOptions options, ITextHandler textHandler, ILogger<TcpServerRoutingService> logger)
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
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task<RoutingServiceResult> RouteAsync(RoutingServiceContext context)
        {
            return Task.Run(async () =>
            {
                if (context.IPEndPoint == null)
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The network IP endpoint is missing in the TCP server routing request.");
                }

                // Start the server in the specified endpoint.
                TcpListener server = new(context.IPEndPoint);
                try
                {
                    server.Start();
                    logger.LogDebug("Server started. endpoint={0} timestamp={1}", context.IPEndPoint, DateTimeOffset.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss.fff"));

                    // Accept messages till cancellation token
                    while (true)
                    {
                        await Task.Delay(options.Router.SyncDelay.GetValueOrDefault());

                        // Honor the cancellation request.
                        if (context.CancellationToken.IsCancellationRequested)
                        {
                            ErrorHandlerContext errContext = new(new Shared.Infrastructure.Error(Errors.RequestCanceled, "Received cancellation token, the routing is canceled."));
                            await errorHandler.HandleAsync(errContext);

                            // We are done, break the loop and stop the server.
                            break;
                        }

                        // Check if application is stopping
                        if (applicationLifetime.ApplicationStopping.IsCancellationRequested)
                        {
                            ErrorHandlerContext errContext = new(new Shared.Infrastructure.Error(Errors.RequestCanceled, $"Application is stopping, the routing is canceled."));
                            await errorHandler.HandleAsync(errContext);

                            // We are done, break the loop and stop the server.
                            break;
                        }

                        // Accept a client connections
                        logger.LogDebug("Initializing {0} client connections.", options.Router.MaxClients);
                        List<Task> clientConnections = new();
                        for (int idx = 0; idx < options.Router.MaxClients; ++idx)
                        {
                            clientConnections.Add(Task.Run(async () =>
                            {
                                using (TcpClient tcpClient = await server.AcceptTcpClientAsync())
                                {
                                    TcpConnectionData tcpConnection = new(server, tcpClient, context.CancellationToken);
                                    await HandleClientConnectedAync(tcpConnection);
                                }
                            }));
                        }
                        await Task.WhenAll(clientConnections);
                    }
                }
                finally
                {
                    server.Stop();
                    logger.LogDebug("Server stopped. endpoint={0} timestamp={1}", context.IPEndPoint, DateTimeOffset.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss.fff"));
                }

                return new RoutingServiceResult();
            });
        }

        private async Task HandleClientConnectedAync(TcpConnectionData connectionData)
        {
            // Honor the max limit
            if (connectionData.Client.Available > options.Router.MaxCommandStringLength)
            {
                throw new ErrorException(Error.InvalidConfiguration, "The command string length is over the configured limit. max_length={0}", options.Router.MaxCommandStringLength);
            }

            // While the client is connected we will route the commands.
            logger.LogDebug("Client connected. client_endpoint={0} server_endpoint={1}", connectionData.Client.Client.LocalEndPoint.ToString(), connectionData.Server.LocalEndpoint.ToString());
            while (connectionData.Client.Connected)
            {
                // Retrieve the TCP connection data, so we can query the connected client.
                if (connectionData.CancellationToken.IsCancellationRequested)
                {
                    break;
                }

                // Read the sent command.
                string? raw;
                try
                {
                    // Get a stream object for reading and writing
                    NetworkStream stream = connectionData.Client.GetStream();
                    stream.ReadTimeout = options.Router.ReadTimeout;
                    byte[] buffer = new byte[options.Router.MaxCommandStringLength];
                    int end = stream.Read(buffer, 0, buffer.Length);
                    if (end == 0)
                    {
                        throw new ErrorException(Errors.ConnectionClosed, "The client socket connection is closed.");
                    }

                    // Get command string, Null or empty, ignore
                    raw = textHandler.Encoding.GetString(buffer, 0, end);
                    if (string.IsNullOrEmpty(raw))
                    {
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug("Client disconnected. client_endpoint={0} server_endpoint={1} additional_info={2}", connectionData.Client.Client.LocalEndPoint.ToString(), connectionData.Server.LocalEndpoint.ToString(), ex.Message);
                    break;
                }

                try
                {
                    // Route the command request to router.
                    CommandRouterContext context = new(raw!, connectionData.CancellationToken);
                    Task<CommandRouterResult> routeTask = commandRouter.RouteAsync(context);

                    // Wait for the router or the timeout.
                    bool success = routeTask.Wait(options.Router.Timeout, connectionData.CancellationToken);
                    if (!success)
                    {
                        throw new TimeoutException($"The command router timed out in {options.Router.Timeout} milliseconds.");
                    }
                }
                catch (Exception ex)
                {
                    // Task.Wait bundles up any exception into Exception.InnerException
                    ExceptionHandlerContext exContext = new(raw!, ex.InnerException ?? ex);
                    await exceptionHandler.HandleAsync(exContext);
                }
            }
        }
    }
}