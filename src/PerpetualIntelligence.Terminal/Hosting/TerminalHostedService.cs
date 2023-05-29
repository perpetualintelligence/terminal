/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Commands.Checkers;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Licensing;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Licensing;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Hosting
{
    /// <summary>
    /// The <c>pi-cli</c> hosted service to manage the application lifetime and terminal customization.
    /// </summary>
    public class TerminalHostedService : IHostedService
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public TerminalHostedService(IServiceProvider serviceProvider, TerminalOptions options, ILogger<TerminalHostedService> logger)
        {
            this.hostApplicationLifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
            this.serviceProvider = serviceProvider;
            this.options = options;
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
                await CheckHostApplicationMandatoryConfigurationAsync(options);

                // Do custom configuration check
                await CheckHostApplicationConfigurationAsync(options);

                // Register the help options with command descriptors. This is intentionally done at the end so we don't take
                // performance hit in case there is a license check failure.
                await RegisterHelpAsync();
            }
            catch (ErrorException ex)
            {
                logger.LogError($"{ex.Error.ErrorCode}={ex.Error.FormatDescription()}");
            }
        }

        /// <summary>
        /// Registers the help options based on configuration options.
        /// </summary>
        /// <returns></returns>
        internal virtual Task RegisterHelpAsync()
        {
            if (options.Help.Disabled.GetValueOrDefault())
            {
                return Task.CompletedTask;
            }

            return Task.Run(() =>
            {
                // This can be a long list of command, but it is executed only once during startup.
                IEnumerable<CommandDescriptor> commandDescriptors = serviceProvider.GetServices<CommandDescriptor>();
                foreach (CommandDescriptor commandDescriptor in commandDescriptors)
                {
                    commandDescriptor.OptionDescriptors ??= new OptionDescriptors(serviceProvider.GetRequiredService<ITextHandler>());

                    OptionDescriptor helpDescriptor = new(options.Help.OptionId, nameof(Boolean), options.Help.OptionDescription) { Alias = options.Help.OptionAlias };
                    commandDescriptor.OptionDescriptors.Add(helpDescriptor);
                }
            });
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
            if (license.Plan == LicensePlans.Demo)
            {
                if (license.Usage == LicenseUsages.Educational)
                {
                    logger.LogInformation("Your community license plan is free for educational purposes. For non-educational or production environment, you require a commercial license.");
                }
                else if (license.Usage == LicenseUsages.RnD)
                {
                    logger.LogInformation("Your community license plan is free for RnD, test, and demo purposes. For production environment, you require a commercial license.");
                }
            }
            else if (license.Plan == LicensePlans.Custom)
            {
                if (license.Usage == LicenseUsages.RnD)
                {
                    logger.LogInformation("Your demo license is free for RnD, test and evaluation purposes. For production environment, you require a commercial license.");
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Allows the host application to perform additional configuration option checks.
        /// </summary>
        /// <returns></returns>
        protected virtual Task CheckHostApplicationConfigurationAsync(TerminalOptions options)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Triggered when the <c>pi-cli</c> application host has fully started.
        /// </summary>
        protected virtual void OnStarted()
        {
            logger.LogInformation("Server started on {0}.", DateTime.UtcNow.ToLocalTime().ToString());
            logger.LogInformation("");
        }

        /// <summary>
        /// Triggered when the <c>pi-cli</c> application host has completed a graceful shutdown. The application will
        /// not exit until all callbacks registered on this token have completed.
        /// </summary>
        protected virtual void OnStopped()
        {
            logger.LogInformation("Server stopped on {0}.", DateTime.UtcNow.ToLocalTime().ToString());
        }

        /// <summary>
        /// Triggered when the <c>pi-cli</c> application host is starting a graceful shutdown. Shutdown will block until
        /// all callbacks registered on this token have completed.
        /// </summary>
        protected virtual void OnStopping()
        {
            logger.LogInformation("Stopping server...");
        }

        /// <summary>
        /// Allows the host application to print the custom header.
        /// </summary>
        protected virtual Task PrintHostApplicationHeaderAsync()
        {
            logger.LogInformation("---------------------------------------------------------------------------------------------");
            logger.LogInformation("Demo custom header line-1");
            logger.LogInformation("Demo custom header line-2");
            logger.LogInformation("---------------------------------------------------------------------------------------------");

            logger.LogInformation($"Starting server \"{Shared.Constants.CliUrn}\" version={typeof(TerminalHostedService).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? " < none > "}");
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
            logger.LogInformation($"consumer={license.Claims.Name} ({license.Claims.TenantId})");
            logger.LogInformation($"country={license.Claims.TenantCountry}");
            logger.LogInformation($"subject={options.Licensing.Subject}");
            logger.LogInformation($"license_handler={license.Handler}");
            logger.LogInformation($"usage={license.Usage}");
            logger.LogInformation($"plan={license.Plan}");
            logger.LogInformation($"key_source={options.Licensing.KeySource}");
            if (license.LicenseKeySource == LicenseSources.JsonFile)
            {
                // Don't dump the key, just the lic file path
                logger.LogInformation($"key_file={license.LicenseKey}");
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
        /// configuration options, but they can perform additional configuration checks with <see cref="CheckHostApplicationConfigurationAsync(TerminalOptions)"/>.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <returns></returns>
        private async Task CheckHostApplicationMandatoryConfigurationAsync(TerminalOptions options)
        {
            IConfigurationOptionsChecker optionsChecker = serviceProvider.GetRequiredService<IConfigurationOptionsChecker>();
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

        private readonly TerminalOptions options;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly ILogger<TerminalHostedService> logger;
        private readonly IServiceProvider serviceProvider;
    }
}