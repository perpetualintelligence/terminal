/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Extensions;
using PerpetualIntelligence.Cli.Licensing;
using PerpetualIntelligence.Cli.Services;
using PerpetualIntelligence.Protocols.Licensing;
using System;
using System.Reflection;
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
        /// <param name="licenseExtractor">The license extractor.</param>
        /// <param name="cliOptions">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public CliHostedService(IHost host, IHostApplicationLifetime hostApplicationLifetime, ILicenseExtractor licenseExtractor, CliOptions cliOptions, ILogger<CliHostedService> logger)
        {
            this.host = host;
            this.hostApplicationLifetime = hostApplicationLifetime;
            this.licenseExtractor = licenseExtractor;
            this.cliOptions = cliOptions;
            this.logger = logger;

            // For canceling.
            cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Starts the cli hosted service.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Print Header
            await PrintHostApplicationHeaderAsync();

            // Register Application Lifetime events
            await RegisterHostApplicationEventsAsync(hostApplicationLifetime);

            // License check, mandatory licensing will print the Community disclaimer.
            LicenseExtractorResult result = await licenseExtractor.ExtractAsync(new LicenseExtractorContext());
            await PrintHostApplicationLicensingAsync(result.License);
            await PrintHostApplicationMandatoryLicensingAsync(result.License);
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
            Console.WriteLine("Server started on {0}.", DateTime.UtcNow.ToLocalTime().ToString());
            Console.WriteLine();

            // The console loop for routing commands.
            logger.LogInformation("Starting command routing...");
            host.RunRouterAsync("cmd > ", cliOptions.Hosting.CommandRouterTimeout, cancellationTokenSource.Token).GetAwaiter().GetResult();

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

        /// <summary>
        /// Allows the host application to print the custom header.
        /// </summary>
        protected virtual Task PrintHostApplicationHeaderAsync()
        {
            Console.WriteLine("---------------------------------------------------------------------------------------------");
            Console.WriteLine("Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.");
            Console.WriteLine("For license, terms, and data policies, go to:");
            Console.WriteLine("https://terms.perpetualintelligence.com");
            Console.WriteLine("---------------------------------------------------------------------------------------------");

            Console.WriteLine($"Starting server \"{Protocols.Constants.CliUrn}\" version={typeof(CliHostedService).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? " < none > "}...");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Allows host application to print custom licensing information.
        /// </summary>
        /// <param name="license"></param>
        /// <returns></returns>
        protected virtual Task PrintHostApplicationLicensingAsync(License license)
        {
            // Print the license information
            ConsoleHelper.WriteLineColor($"Consumer={license.Claims.Name} ({license.Claims.TenantId})", ConsoleColor.Cyan);
            ConsoleHelper.WriteLineColor($"Country={license.Claims.TenantCountry}", ConsoleColor.Cyan);
            ConsoleHelper.WriteLineColor($"Subject={cliOptions.Licensing.Subject}", ConsoleColor.Cyan);
            ConsoleHelper.WriteLineColor($"Key Source={cliOptions.Licensing.KeySource}", ConsoleColor.Cyan);
            if (cliOptions.Licensing.KeySource == LicenseKeySource.JsonFile)
            {
                // Don't dump the key, just the lic file path
                ConsoleHelper.WriteLineColor($"Key File={license.LicenseKey}", ConsoleColor.Cyan);
            }
            ConsoleHelper.WriteLineColor($"Check={license.CheckMode}", ConsoleColor.Cyan);
            ConsoleHelper.WriteLineColor($"Usage={license.Usage}", ConsoleColor.Cyan);
            ConsoleHelper.WriteLineColor($"Edition={license.Plan}", ConsoleColor.Cyan);

            return Task.CompletedTask;
        }

        /// <summary>
        /// This method prints the mandatory licensing details. Applications cannot customize or change the mandatory
        /// licensing information, but they can print additional custom information with <see cref="PrintHostApplicationLicensingAsync(License)"/>.
        /// </summary>
        /// <param name="license">The extracted license.</param>
        protected Task PrintHostApplicationMandatoryLicensingAsync(License license)
        {
            if (license.Plan == SaaSPlans.Community)
            {
                if (license.Usage == SaaSUsages.Educational)
                {
                    ConsoleHelper.WriteLineColor("The community edition is free for educational purposes only. For commercial and non-educational use, you require a paid subscription.", ConsoleColor.Yellow);
                }
                else if (license.Usage == SaaSUsages.RnD)
                {
                    ConsoleHelper.WriteLineColor("The community edition is free for RnD, test, and demo purposes only. For commercial or production use, you require a paid subscription.", ConsoleColor.Yellow);
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Allows the application to register its custom <see cref="IHostApplicationLifetime"/> events.
        /// </summary>
        protected virtual Task RegisterHostApplicationEventsAsync(IHostApplicationLifetime hostApplicationLifetime)
        {
            // Print Header
            hostApplicationLifetime.ApplicationStarted.Register(OnStarted);
            hostApplicationLifetime.ApplicationStopping.Register(OnStopping);
            hostApplicationLifetime.ApplicationStopped.Register(OnStopped);
            return Task.CompletedTask;
        }

        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly CliOptions cliOptions;
        private readonly IHost host;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly ILicenseExtractor licenseExtractor;
        private readonly ILogger<CliHostedService> logger;
    }
}
