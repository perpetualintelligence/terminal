/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Infrastructure;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Commands.Routers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalRouting{TContext, TResult}"/> for TCP client server communication.
    /// </summary>
    public class TcpRouting : ITerminalRouting<TcpRoutingContext, TcpRoutingResult>
    {
        private readonly IHostApplicationLifetime applicationLifetime;
        private readonly ICommandRouter commandRouter;
        private readonly IExceptionHandler exceptionHandler;
        private readonly IErrorHandler errorHandler;
        private readonly TerminalOptions options;
        private readonly ITextHandler textHandler;
        private readonly ILogger<TcpRouting> logger;

        /// <summary>
        /// Initialize a new <see cref="TcpRouting"/> instance.
        /// </summary>
        /// <param name="applicationLifetime">The host application lifetime instance.</param>
        /// <param name="commandRouter">The command router.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <param name="errorHandler">The error handler.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="logger">The logger.</param>
        public TcpRouting(
            IHostApplicationLifetime applicationLifetime,
            ICommandRouter commandRouter,
            IExceptionHandler exceptionHandler,
            IErrorHandler errorHandler,
            TerminalOptions options,
            ITextHandler textHandler,
            ILogger<TcpRouting> logger)
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
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual Task<TcpRoutingResult> RunAsync(TcpRoutingContext context)
        {
            //  Make sure we have supported start context
            if (context.StartContext.StartInformation.StartMode != TerminalStartMode.Tcp)
            {
                throw new ErrorException(TerminalErrors.InvalidConfiguration, "The requested start mode is not valid for console routing. start_mode={0}", context.StartContext.StartInformation.StartMode);
            }

            return Task.Run(async () =>
            {
                if (context is not TcpRoutingContext tcpContext)
                {
                    throw new ErrorException(TerminalErrors.InvalidConfiguration, "The routing service context is not valid for TCP routing service.");
                }

                if (tcpContext.IPEndPoint == null)
                {
                    throw new ErrorException(TerminalErrors.InvalidConfiguration, "The network IP endpoint is missing in the TCP server routing request.");
                }

                // Start the server in the specified endpoint.
                TcpListener server = new(tcpContext.IPEndPoint);
                try
                {
                    server.Start();
                    logger.LogDebug("TCP Server started. endpoint={0} timestamp={1}", tcpContext.IPEndPoint, DateTimeOffset.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss.fff"));

                    // Accept messages till cancellation token
                    while (true)
                    {
                        await Task.Delay(options.Router.SyncDelay.GetValueOrDefault());

                        // Honor the cancellation request.
                        if (tcpContext.StartContext.CancellationToken.IsCancellationRequested)
                        {
                            ErrorHandlerContext errContext = new(new Error(TerminalErrors.RequestCanceled, "Received cancellation token, the routing is canceled."));
                            await errorHandler.HandleAsync(errContext);

                            // We are done, break the loop and stop the server.
                            break;
                        }

                        // Check if application is stopping
                        if (applicationLifetime.ApplicationStopping.IsCancellationRequested)
                        {
                            ErrorHandlerContext errContext = new(new Error(TerminalErrors.RequestCanceled, $"Application is stopping, the routing is canceled."));
                            await errorHandler.HandleAsync(errContext);

                            // We are done, break the loop and stop the server.
                            break;
                        }

                        // Accept a client connections
                        logger.LogDebug("Initializing {0} client connections.", options.Router.RemoteMaxClients);
                        List<Task> clientConnections = new();
                        for (int idx = 0; idx < options.Router.RemoteMaxClients; ++idx)
                        {
                            int localIdx = idx + 1;
                            clientConnections.Add(Task.Run(async () =>
                            {
                                logger.LogDebug("Waiting for client to connect on task {0}", localIdx);
                                using (TcpClient tcpClient = await server.AcceptTcpClientAsync())
                                {
                                    tcpContext.Setup(server, tcpClient);
                                    await HandleClientConnectedAsync(tcpContext);
                                }
                            }));
                        }
                        await Task.WhenAll(clientConnections);
                        logger.LogWarning("All client connection tasks are exhausted.");
                    }
                }
                finally
                {
                    server.Stop();
                    logger.LogDebug("TCP server stopped. endpoint={0} timestamp={1}", tcpContext.IPEndPoint, DateTimeOffset.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss.fff"));
                }

                // Return the result
                return new TcpRoutingResult();
            });
        }

        private async Task HandleClientConnectedAsync(TcpRoutingContext tcpContext)
        {
            if (tcpContext.Server == null || tcpContext.Client == null)
            {
                throw new ErrorException(Error.InvalidConfiguration, "The TCP routing context is not setup with a client and server.");
            }

            // Honor the max limit
            if (tcpContext.Client.Available > options.Router.MaxCommandStringLength)
            {
                throw new ErrorException(Error.InvalidConfiguration, "The command string length is over the configured limit. max_length={0}", options.Router.MaxCommandStringLength);
            }

            // While the client is connected we will route the commands.
            logger.LogDebug("Client connected. client_endpoint={0} server_endpoint={1}", tcpContext.Client.Client.LocalEndPoint.ToString(), tcpContext.Server.LocalEndpoint.ToString());
            while (tcpContext.Client.Connected)
            {
                // Retrieve the TCP connection data, so we can query the connected client.
                if (tcpContext.StartContext.CancellationToken.IsCancellationRequested)
                {
                    break;
                }

                // Read the sent command.
                string? raw;
                try
                {
                    // Get a stream object for reading and writing
                    using (NetworkStream stream = tcpContext.Client.GetStream())
                    {
                        byte[] buffer = new byte[options.Router.MaxCommandStringLength];
                        int end = stream.Read(buffer, 0, buffer.Length);
                        if (end == 0)
                        {
                            throw new ErrorException(TerminalErrors.ConnectionClosed, "The client socket connection is closed.");
                        }

                        // Get command string, Null or empty, ignore
                        raw = textHandler.Encoding.GetString(buffer, 0, end);
                    }

                    if (string.IsNullOrEmpty(raw))
                    {
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug("Client disconnected. client_endpoint={0} server_endpoint={1} info={2}", tcpContext.Client.Client.LocalEndPoint.ToString(), tcpContext.Server.LocalEndpoint.ToString(), ex.Message);
                    break;
                }

                try
                {
                    // Route the command request to router.
                    CommandRouterContext context = new(raw!, tcpContext.StartContext.CancellationToken);
                    Task<CommandRouterResult> routeTask = commandRouter.RouteAsync(context);

                    // Wait for the router or the timeout.
                    bool success = routeTask.Wait(options.Router.Timeout, tcpContext.StartContext.CancellationToken);
                    if (!success)
                    {
                        throw new TimeoutException($"The command router timed out in {options.Router.Timeout} milliseconds.");
                    }

                    // This means a success in command runner. Wait for the next command.
                    // Dispose the runner result, it is not propagated any further.
                    await routeTask.Result.HandlerResult.RunnerResult.DisposeAsync();
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