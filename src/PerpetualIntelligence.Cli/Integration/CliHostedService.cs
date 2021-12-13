/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Integration
{
    /// <summary>
    /// The <c>cli</c> hosted service to manage the application lifetime.
    /// </summary>
    public class CliHostedService : IHostedService
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="hostApplicationLifetime">The host application lifetime.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public CliHostedService(IHost host, IHostApplicationLifetime hostApplicationLifetime, CliOptions options, ILogger<CliHostedService> logger)
        {
            this.host = host;
            this.hostApplicationLifetime = hostApplicationLifetime;
            this.options = options;
            this.logger = logger;

            // For canceling.
            cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            hostApplicationLifetime.ApplicationStarted.Register(OnStarted);
            hostApplicationLifetime.ApplicationStopping.Register(OnStopping);
            hostApplicationLifetime.ApplicationStopped.Register(OnStopped);
            return Task.CompletedTask;
        }

        /// <summary>
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Send cancellation request to router
            cancellationTokenSource.Cancel();

            // Give time for router to end the loop.
            Task.WaitAll(Task.Delay(1000));

            return Task.CompletedTask;
        }

        /// <summary>
        /// </summary>
        protected virtual void OnStarted()
        {
            Console.WriteLine("Server started.");
            Console.WriteLine();

            // The console loop for routing commands.
            logger.LogInformation("Starting command routing...");
            host.RunRouterAsync("cmd > ", options.Hosting.CommandRouterTimeout, cancellationTokenSource.Token).GetAwaiter().GetResult();

            // Closing comments
            logger.LogWarning("Command routing stopped.");

            Task.WaitAll(Task.Delay(500));

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Server stopped.");
            Console.ResetColor();
        }

        /// <summary>
        /// </summary>
        protected virtual void OnStopped()
        {
        }

        /// <summary>
        /// </summary>
        protected virtual void OnStopping()
        {
            Console.WriteLine("Stopping server...");

            Task.WaitAll(Task.Delay(500));
        }

        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly IHost host;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly ILogger<CliHostedService> logger;
        private readonly CliOptions options;
    }
}
