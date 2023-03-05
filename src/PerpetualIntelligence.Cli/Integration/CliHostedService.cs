/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Licensing;
using PerpetualIntelligence.Cli.Services;
using PerpetualIntelligence.Shared.Licensing;
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
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="cliOptions">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public CliHostedService(IServiceProvider serviceProvider, CliOptions cliOptions, ILogger<CliHostedService> logger)
        {
            this.hostApplicationLifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
            this.serviceProvider = serviceProvider;
            this.cliOptions = cliOptions;
            this.logger = logger;
        }

        /// <summary>
        /// Starts the <c>pi-cli</c> hosted service asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // We catch the exception to avoid unhandeled fatal exception
            try
            {
                // Register Application Lifetime events
                await RegisterHostApplicationEventsAsync(hostApplicationLifetime);

                // Print Header
                await PrintHostApplicationHeaderAsync();

                // Extract license
                LicenseExtractorResult result = await ExtractLicenseAsync();

                // We have extracted the license, print lic info
                await PrintHostApplicationLicensingAsync(result.License);

                // Do license check
                await CheckLicenseAsync(result);

                // Print mandatory licensing for community and demo license
                await PrintHostApplicationMandatoryLicensingAsync(result.License);

                // Do mandatory configuration check
                await CheckHostApplicationMandatoryConfigurationAsync(cliOptions);

                // Do custom configuration check
                await CheckHostApplicationConfigurationAsync(cliOptions);
            }
            catch (ErrorException ex)
            {
                ConsoleHelper.WriteLineColor(ConsoleColor.Red, $"{ex.Error.ErrorCode}={ex.Error.FormatDescription()}");
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
        /// This method prints the mandatory licensing details. Applications cannot customize or change the mandatory
        /// licensing information, but they can print additional custom information with <see cref="PrintHostApplicationLicensingAsync(License)"/>.
        /// </summary>
        /// <param name="license">The extracted license.</param>
        internal virtual Task PrintHostApplicationMandatoryLicensingAsync(License license)
        {
            if (license.Plan == LicensePlans.Community)
            {
                if (license.Usage == LicenseUsages.Educational)
                {
                    ConsoleHelper.WriteLineColor(ConsoleColor.Yellow, "Your community license plan is free for educational purposes. For non-educational or production environment, you require a commercial license.");
                }
                else if (license.Usage == LicenseUsages.RnD)
                {
                    ConsoleHelper.WriteLineColor(ConsoleColor.Yellow, "Your community license plan is free for RnD, test, and demo purposes. For production environment, you require a commercial license.");
                }
            }
            else if (license.Plan == LicensePlans.Custom)
            {
                if (license.Usage == LicenseUsages.RnD)
                {
                    ConsoleHelper.WriteLineColor(ConsoleColor.Yellow, "Your demo license is free for RnD, test and evaluation purposes. For production environment, you require a commercial license.");
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Allows the host application to perform additional configuration option checks.
        /// </summary>
        /// <returns></returns>
        protected virtual Task CheckHostApplicationConfigurationAsync(CliOptions options)
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

            Console.WriteLine($"Starting server \"{Shared.Constants.CliUrn}\" version={typeof(CliHostedService).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? " < none > "}");
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
            ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, $"license_handler={license.Handler}");
            ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, $"usage={license.Usage}");
            ConsoleHelper.WriteLineColor(ConsoleColor.Green, $"plan={license.Plan}");
            ConsoleHelper.WriteLineColor(ConsoleColor.Cyan, $"key_source={cliOptions.Licensing.KeySource}");
            if (license.LicenseKeySource == LicenseSources.JsonFile)
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
        /// This method check the mandatory configuration options. Applications cannot customize or change the mandatory
        /// configuration options, but they can perform additional configuration checks with <see cref="CheckHostApplicationConfigurationAsync(CliOptions)"/>.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <returns></returns>
        private async Task CheckHostApplicationMandatoryConfigurationAsync(CliOptions options)
        {
            IOptionsChecker optionsChecker = serviceProvider.GetRequiredService<IOptionsChecker>();
            await optionsChecker.CheckAsync(options);
        }

        private async Task CheckLicenseAsync(LicenseExtractorResult result)
        {
            ILicenseChecker licenseChecker = serviceProvider.GetRequiredService<ILicenseChecker>();
            await licenseChecker.CheckAsync(new LicenseCheckerContext(result.License));
        }

        private async Task<LicenseExtractorResult> ExtractLicenseAsync()
        {
            ILicenseExtractor licenseExtractor = serviceProvider.GetRequiredService<ILicenseExtractor>();
            LicenseExtractorResult result = await licenseExtractor.ExtractAsync(new LicenseExtractorContext());
            return result;
        }

        private readonly CliOptions cliOptions;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly ILogger<CliHostedService> logger;
        private readonly IServiceProvider serviceProvider;
    }
}
