/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Shared.Attributes;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Infrastructure;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Extensions
{
    /// <summary>
    /// The <see cref="IHost"/> extension methods.
    /// </summary>
    public static class IHostExtensions
    {
        /// <summary>
        /// Returns a task that runs the <see cref="ICommandRouter"/> as a terminal console that blocks the calling thread till a cancellation request.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        [WriteDocumentation("Add info about exception handling for ErrorException")]
        public static Task<RoutingServiceResult> RunRouterAsTerminalAsync(this IHost host, CancellationToken cancellationToken)
        {
            IRoutingService routingService = host.Services.GetRequiredService<IRoutingService>();
            if (routingService is ConsoleTerminalRoutingService consoleTerminalRouting)
            {
                RoutingServiceContext context = new(cancellationToken);
                return consoleTerminalRouting.RouteAsync(context);
            }

            throw new ErrorException(Errors.InvalidConfiguration, "The configured routing service is not a console routing service.");
        }

        /// <summary>
        /// Returns a task that runs the <see cref="ICommandRouter"/> that listens for connections from TCP network clients and blocks the calling thread till a cancellation token.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="iPEndPoint">The network endpoint as an IP address and a port number.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task RunRouterAsTcpServerAsync(this IHost host, IPEndPoint iPEndPoint, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                // Track the application lifetime so we can know whether cancellation is requested.
                IHostApplicationLifetime applicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
                CliOptions cliOptions = host.Services.GetRequiredService<CliOptions>();
                ILogger<CommandRouter> logger = host.Services.GetRequiredService<ILogger<CommandRouter>>();

                // Start the server in the specified endpoint.
                TcpListener server = new(iPEndPoint);
                try
                {
                    server.Start();
                    logger.LogDebug("Server started. endpoint={0} timestamp={1}", iPEndPoint, DateTimeOffset.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss.fff"));

                    // Accept messages till cancellation token
                    while (true)
                    {
                        await Task.Delay(cliOptions.Router.SyncDelay.GetValueOrDefault());

                        // Honor the cancellation request.
                        if (cancellationToken.IsCancellationRequested)
                        {
                            IErrorHandler errorPublisher = host.Services.GetRequiredService<IErrorHandler>();
                            ErrorHandlerContext errContext = new(new Shared.Infrastructure.Error(Errors.RequestCanceled, "Received cancellation token, the routing is canceled."));
                            await errorPublisher.HandleAsync(errContext);

                            // We are done, break the loop and stop the server.
                            break;
                        }

                        // Check if application is stopping
                        if (applicationLifetime.ApplicationStopping.IsCancellationRequested)
                        {
                            IErrorHandler errorPublisher = host.Services.GetRequiredService<IErrorHandler>();
                            ErrorHandlerContext errContext = new(new Shared.Infrastructure.Error(Errors.RequestCanceled, $"Application is stopping, the routing is canceled."));
                            await errorPublisher.HandleAsync(errContext);

                            // We are done, break the loop and stop the server.
                            break;
                        }

                        // Accept a client connections
                        logger.LogDebug("Initializing {0} client connections.", cliOptions.Router.MaxClients);
                        List<Task> clientConnections = new();
                        for (int idx = 0; idx < cliOptions.Router.MaxClients; ++idx)
                        {
                            clientConnections.Add(Task.Run(async () =>
                            {
                                using (TcpClient tcpClient = await server.AcceptTcpClientAsync())
                                {
                                    TcpConnectionData tcpConnection = new(server, tcpClient, host, cliOptions, logger, cancellationToken);
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
                    logger.LogDebug("Server stopped. endpoint={0} timestamp={1}", iPEndPoint, DateTimeOffset.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss.fff"));
                }
            });
        }

        /// <summary>
        /// Returns a task that runs the <see cref="ICommandRouter"/> that starts a custom service and blocks the calling thread till a cancellation token.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task RunRouterAsCustomServiceAsync(this IHost host, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                // Track the application lifetime so we can know whether cancellation is requested.
                IHostApplicationLifetime applicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
                CliOptions cliOptions = host.Services.GetRequiredService<CliOptions>();
                ILogger<CommandRouter> logger = host.Services.GetRequiredService<ILogger<CommandRouter>>();

                // Start the server in the specified endpoint.
                TcpListener server = new(iPEndPoint);
                try
                {
                    server.Start();
                    logger.LogDebug("Server started. endpoint={0} timestamp={1}", iPEndPoint, DateTimeOffset.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss.fff"));

                    // Accept messages till cancellation token
                    while (true)
                    {
                        await Task.Delay(cliOptions.Router.SyncDelay.GetValueOrDefault());

                        // Honor the cancellation request.
                        if (cancellationToken.IsCancellationRequested)
                        {
                            IErrorHandler errorPublisher = host.Services.GetRequiredService<IErrorHandler>();
                            ErrorHandlerContext errContext = new(new Shared.Infrastructure.Error(Errors.RequestCanceled, "Received cancellation token, the routing is canceled."));
                            await errorPublisher.HandleAsync(errContext);

                            // We are done, break the loop and stop the server.
                            break;
                        }

                        // Check if application is stopping
                        if (applicationLifetime.ApplicationStopping.IsCancellationRequested)
                        {
                            IErrorHandler errorPublisher = host.Services.GetRequiredService<IErrorHandler>();
                            ErrorHandlerContext errContext = new(new Shared.Infrastructure.Error(Errors.RequestCanceled, $"Application is stopping, the routing is canceled."));
                            await errorPublisher.HandleAsync(errContext);

                            // We are done, break the loop and stop the server.
                            break;
                        }

                        // Accept a client connections
                        logger.LogDebug("Initializing {0} client connections.", cliOptions.Router.MaxClients);
                        List<Task> clientConnections = new();
                        for (int idx = 0; idx < cliOptions.Router.MaxClients; ++idx)
                        {
                            clientConnections.Add(Task.Run(async () =>
                            {
                                using (TcpClient tcpClient = await server.AcceptTcpClientAsync())
                                {
                                    TcpConnectionData tcpConnection = new(server, tcpClient, host, cliOptions, logger, cancellationToken);
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
                    logger.LogDebug("Server stopped. endpoint={0} timestamp={1}", iPEndPoint, DateTimeOffset.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss.fff"));
                }
            });
        }

        private static async Task HandleClientConnectedAync(TcpConnectionData connectionData)
        {
            ITextHandler textHandler = connectionData.Host.Services.GetRequiredService<ITextHandler>();
            CliOptions options = connectionData.Options;

            // Honor the max limit
            if (connectionData.Client.Available > options.Router.MaxCommandStringLength)
            {
                throw new ErrorException(Error.InvalidConfiguration, "The command string length is over the configured limit. max_length={0}", options.Router.MaxCommandStringLength);
            }

            // While the client is connected we will route the commands.
            connectionData.Logger.LogDebug("Client connected. client_endpoint={0} server_endpoint={1}", connectionData.Client.Client.LocalEndPoint.ToString(), connectionData.Server.LocalEndpoint.ToString());
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
                    stream.ReadTimeout = connectionData.Options.Router.ReadTimeout;
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
                    connectionData.Logger.LogDebug("Client disconnected. client_endpoint={0} server_endpoint={1} additional_info={2}", connectionData.Client.Client.LocalEndPoint.ToString(), connectionData.Server.LocalEndpoint.ToString(), ex.Message);
                    break;
                }

                try
                {
                    // Route the command request to router.
                    CommandRouterContext context = new(raw!, connectionData.CancellationToken);
                    ICommandRouter router = connectionData.Host.Services.GetRequiredService<ICommandRouter>();
                    Task<CommandRouterResult> routeTask = router.RouteAsync(context);

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
                    IExceptionHandler exceptionPublisher = connectionData.Host.Services.GetRequiredService<IExceptionHandler>();
                    ExceptionHandlerContext exContext = new(raw!, ex.InnerException ?? ex);
                    await exceptionPublisher.HandleAsync(exContext);
                }
            }
        }
    }
}