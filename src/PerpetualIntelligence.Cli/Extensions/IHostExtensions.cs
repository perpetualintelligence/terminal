/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Shared.Attributes;
using System;
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
        /// <param name="caret">The command caret to show in the console.</param>
        [WriteDocumentation("Add info about exception handling for ErrorException")]
        public static Task RunRouterAsTerminalAsync(this IHost host, string? caret = null, CancellationToken? cancellationToken = default)
        {
            return Task.Run(async () =>
            {
                // Track the application lifetime so we can know whether cancellation is requested.
                IHostApplicationLifetime applicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
                CliOptions cliOptions = host.Services.GetRequiredService<CliOptions>();

                while (true)
                {
                    // Avoid block threads during cancellation and let the
                    // applicationLifetime.ApplicationStopping.IsCancellationRequested get synchronized so we can honor the
                    // app shutdown
                    await Task.Delay(200);

                    // Honor the cancellation request.
                    if (cancellationToken.GetValueOrDefault().IsCancellationRequested)
                    {
                        IErrorHandler errorPublisher = host.Services.GetRequiredService<IErrorHandler>();
                        ErrorHandlerContext errContext = new(new Shared.Infrastructure.Error(Errors.RequestCanceled, "Received cancellation token, the routing is canceled."));
                        await errorPublisher.HandleAsync(errContext);

                        // We are done, break the loop.
                        break;
                    }

                    // Check if application is stopping
                    if (applicationLifetime.ApplicationStopping.IsCancellationRequested)
                    {
                        IErrorHandler errorPublisher = host.Services.GetRequiredService<IErrorHandler>();
                        ErrorHandlerContext errContext = new(new Shared.Infrastructure.Error(Errors.RequestCanceled, $"Application is stopping, the routing is canceled."));
                        await errorPublisher.HandleAsync(errContext);

                        // We are done, break the loop.
                        break;
                    }

                    // Print the caret
                    if (caret != null)
                    {
                        Console.Write(caret);
                    }

                    // Read the user input
                    string? raw = Console.ReadLine();

                    // Ignore empty commands
                    if (string.IsNullOrWhiteSpace(raw))
                    {
                        // Wait for next command.
                        continue;
                    }

                    try
                    {
                        // Route the request.
                        CommandRouterContext context = new(raw, cancellationToken);
                        ICommandRouter router = host.Services.GetRequiredService<ICommandRouter>();
                        Task<CommandRouterResult> routeTask = router.RouteAsync(context);

                        bool success = routeTask.Wait(cliOptions.Router.Timeout, cancellationToken ?? CancellationToken.None);
                        if (!success)
                        {
                            throw new TimeoutException($"The command router timed out in {cliOptions.Router.Timeout} milliseconds.");
                        }

                        // This means a success in command runner. Wait for the next command
                    }
                    catch (Exception ex)
                    {
                        // Task.Wait bundles up any exception into Exception.InnerException
                        IExceptionHandler exceptionPublisher = host.Services.GetRequiredService<IExceptionHandler>();
                        ExceptionHandlerContext exContext = new(raw, ex.InnerException ?? ex);
                        await exceptionPublisher.HandleAsync(exContext);
                    }
                };
            });
        }

        /// <summary>
        /// Returns a task that runs the <see cref="ICommandRouter"/> that listens for connections from TCP network clients and blocks the calling thread till a cancellation token.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="iPEndPoint">The network endpoint as an IP address and a port number.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task RunRouterAsTcpServerAsync(this IHost host, IPEndPoint iPEndPoint, CancellationToken? cancellationToken = default)
        {
            return Task.Run(async () =>
            {
                // Track the application lifetime so we can know whether cancellation is requested.
                IHostApplicationLifetime applicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
                CliOptions cliOptions = host.Services.GetRequiredService<CliOptions>();
                ITextHandler textHandler = host.Services.GetRequiredService<ITextHandler>();

                // Set the TcpListener on specified endpoint.
                TcpListener server = new(iPEndPoint);

                // Start listening for client requests.
                server.Start();

                // Accept messages till cancellation token
                while (true)
                {
                    // Honor the cancellation request.
                    if (cancellationToken.GetValueOrDefault().IsCancellationRequested)
                    {
                        // Make sure listener is stopped
                        server.Stop();

                        IErrorHandler errorPublisher = host.Services.GetRequiredService<IErrorHandler>();
                        ErrorHandlerContext errContext = new(new Shared.Infrastructure.Error(Errors.RequestCanceled, "Received cancellation token, the routing is canceled."));
                        await errorPublisher.HandleAsync(errContext);

                        // We are done, break the loop.
                        break;
                    }

                    // Perform a blocking call to accept requests.
                    using (TcpClient client = await server.AcceptTcpClientAsync())
                    {
                        // Get a stream object for reading and writing
                        NetworkStream stream = client.GetStream();

                        // Loop to receive all the data sent by the client.
                        int end = 0;
                        byte[] buffer = new byte[1024];
                        string? raw = null;
                        while ((end = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                        {
                            if (raw == null)
                            {
                                raw = textHandler.Encoding.GetString(buffer, 0, end);
                            }
                            else
                            {
                                raw += textHandler.Encoding.GetString(buffer, 0, end);
                            }
                        }

                        // Ignore empty commands
                        if (string.IsNullOrWhiteSpace(raw))
                        {
                            // Wait for next command.
                            continue;
                        }

                        try
                        {
                            // Route the request.
                            CommandRouterContext context = new(raw!, cancellationToken);
                            ICommandRouter router = host.Services.GetRequiredService<ICommandRouter>();
                            Task<CommandRouterResult> routeTask = router.RouteAsync(context);

                            bool success = routeTask.Wait(cliOptions.Router.Timeout, cancellationToken ?? CancellationToken.None);
                            if (!success)
                            {
                                throw new TimeoutException($"The command router timed out in {cliOptions.Router.Timeout} milliseconds.");
                            }

                            // This means a success in command runner. Wait for the next command
                        }
                        catch (Exception ex)
                        {
                            // Make sure listener is stopped
                            server.Stop();

                            // Task.Wait bundles up any exception into Exception.InnerException
                            IExceptionHandler exceptionPublisher = host.Services.GetRequiredService<IExceptionHandler>();
                            ExceptionHandlerContext exContext = new(raw!, ex.InnerException ?? ex);
                            await exceptionPublisher.HandleAsync(exContext);
                        }
                    }
                }
            });
        }
    }
}