/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Licensing;
using PerpetualIntelligence.Cli.Services;
using PerpetualIntelligence.Protocols.Licensing;
using PerpetualIntelligence.Shared.Exceptions;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Integration
{
    /// <summary>
    /// The <c>pi-cli</c> hosted service to manage the application lifetime and terminal customization.
    /// </summary>
    public class CliHostedService : IHostedService
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="hostApplicationLifetime">The host application lifetime.</param>
        /// <param name="licenseExtractor">The license extractor.</param>
        /// <param name="licenseChecker">The license checker.</param>
        /// <param name="cliOptions">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public CliHostedService(IHost host, IHostApplicationLifetime hostApplicationLifetime, ILicenseExtractor licenseExtractor, ILicenseChecker licenseChecker, CliOptions cliOptions, ILogger<CliHostedService> logger)
        {
            this.host = host;
            this.hostApplicationLifetime = hostApplicationLifetime;
            this.licenseExtractor = licenseExtractor;
            this.licenseChecker = licenseChecker;
            this.cliOptions = cliOptions;
            this.logger = logger;
        }

        /// <summary>
        /// Starts the <c>pi-cli</c> hosted service asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Register Application Lifetime events
            await RegisterHostApplicationEventsAsync(hostApplicationLifetime);

            // Print Header
            await PrintHostApplicationHeaderAsync();

            try
            {
                // We catch the exception to avoid unhandeled fatal exception. License extraction
                LicenseExtractorResult result = await licenseExtractor.ExtractAsync(new LicenseExtractorContext());
                await PrintHostApplicationLicensingAsync(result.License);

                // Do license check
                await licenseChecker.CheckAsync(new LicenseCheckerContext(result.License));

                // Check, mandatory licensing will print the Community disclaimer.
                await PrintHostApplicationMandatoryLicensingAsync(result.License);
            }
            catch (ErrorException ex)
            {
                ConsoleHelper.WriteLineColor(ConsoleColor.Red, $"invalid_license={ex.Error.FormatDescription()}");
            }
        }

        /// <summary>
        /// Stops the <c>pi-cli</c> hosted service asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Triggered when the <c>pi-cli</c> application host has fully started.
        /// </summary>
        protected virtual void OnStarted()
        {
            Console.WriteLine("Server started on {0}.", DateTime.UtcNow.ToLocalTime().ToString());
            Console.WriteLine();
        }

        /// <summary>
        /// Triggered when the <c>pi-cli</c> application host has completed a graceful shutdown. The application will
        /// not exit until all callbacks registered on this token have completed.
        /// </summary>
        protected virtual void OnStopped()
        {
            ConsoleHelper.WriteLineColor(ConsoleColor.Red, "Server stopped on {0}.", DateTime.UtcNow.ToLocalTime().ToString());
        }

        /// <summary>
        /// Triggered when the <c>pi-cli</c> application host is starting a graceful shutdown. Shutdown will block until
        /// all callbacks registered on this token have completed.
        /// </summary>
        protected virtual void OnStopping()
        {
            Console.WriteLine("Stopping server...");
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

            Console.WriteLine($"Starting server \"{Protocols.Constants.CliUrn}\" version={typeof(CliHostedService).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? " < none > "}");
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
            ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, $"consumer={license.Claims.Name} ({license.Claims.TenantId})");
            ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, $"country={license.Claims.TenantCountry}");
            ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, $"subject={cliOptions.Licensing.Subject}");
            ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, $"handler={license.Handler}");
            ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, $"usage={license.Usage}");
            ConsoleHelper.WriteLineColor(ConsoleColor.Green, $"edition={license.Plan}");
            ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, $"key_source={cliOptions.Licensing.KeySource}");
            if (license.LicenseKeySource == SaaSKeySources.JsonFile)
            {
                // Don't dump the key, just the lic file path
                ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, $"key_file={license.LicenseKey}");
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Allows the application to register its custom <see cref="IHostApplicationLifetime"/> events.
        /// </summary>
        protected virtual Task RegisterHostApplicationEventsAsync(IHostApplicationLifetime hostApplicationLifetime)
        {
            hostApplicationLifetime.ApplicationStarted.Register(OnStarted);
            hostApplicationLifetime.ApplicationStopping.Register(OnStopping);
            hostApplicationLifetime.ApplicationStopped.Register(OnStopped);
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method prints the mandatory licensing details. Applications cannot customize or change the mandatory
        /// licensing information, but they can print additional custom information with <see cref="PrintHostApplicationLicensingAsync(License)"/>.
        /// </summary>
        /// <param name="license">The extracted license.</param>
        private Task PrintHostApplicationMandatoryLicensingAsync(License license)
        {
            if (license.Plan == SaaSPlans.Community)
            {
                if (license.Usage == SaaSUsages.Educational)
                {
                    ConsoleHelper.WriteLineColor(ConsoleColor.Yellow, "The community edition is free for educational purposes only. For commercial and non-educational use, you require a paid subscription.");
                }
                else if (license.Usage == SaaSUsages.RnD)
                {
                    ConsoleHelper.WriteLineColor(ConsoleColor.Yellow, "The community edition is free for RnD, test, and demo purposes only. For commercial or production use, you require a paid subscription.");
                }
            }

            return Task.CompletedTask;
        }

        private readonly CliOptions cliOptions;
        private readonly IHost host;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly ILicenseChecker licenseChecker;
        private readonly ILicenseExtractor licenseExtractor;
        private readonly ILogger<CliHostedService> logger;
    }
}
