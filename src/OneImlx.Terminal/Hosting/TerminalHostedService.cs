/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Hosting
{
    /// <summary>
    /// The hosted service to manage the application lifetime and terminal customization.
    /// </summary>
    public abstract class TerminalHostedService : IHostedService
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public TerminalHostedService(IServiceProvider serviceProvider, TerminalOptions options, ILogger<TerminalHostedService> logger)
        {
            this.HostApplicationLifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
            this.ServiceProvider = serviceProvider;
            this.Options = options;
            this.Logger = logger;
        }

        /// <summary>
        /// Starts the  hosted service asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // We catch the exception to avoid unhandled fatal exception
            try
            {
                // Register Application Lifetime events
                await RegisterHostApplicationEventsAsync(HostApplicationLifetime);

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
                await CheckHostApplicationMandatoryConfigurationAsync(Options);

                // Do custom configuration check
                await CheckHostApplicationConfigurationAsync(Options);

                // Register the help options with command descriptors. This is intentionally done at the end so we don't take
                // performance hit in case there is a license check failure.
                await RegisterHelpAsync();
            }
            catch (TerminalException ex)
            {
                Logger.LogError($"{ex.Error.ErrorCode}={ex.Error.FormatDescription()}");
            }
        }

        /// <summary>
        /// Registers the help options based on configuration options.
        /// </summary>
        /// <returns></returns>
        internal virtual Task RegisterHelpAsync()
        {
            if (Options.Help.Disabled.GetValueOrDefault())
            {
                return Task.CompletedTask;
            }

            return Task.Run(() =>
            {
                // This can be a long list of command, but it is executed only once during startup.
                IEnumerable<CommandDescriptor> commandDescriptors = ServiceProvider.GetServices<CommandDescriptor>();
                foreach (CommandDescriptor commandDescriptor in commandDescriptors)
                {
                    OptionDescriptor helpDescriptor = new(Options.Help.OptionId, nameof(Boolean), Options.Help.OptionDescription, OptionFlags.None, Options.Help.OptionAlias);

                    commandDescriptor.OptionDescriptors ??= new OptionDescriptors(ServiceProvider.GetRequiredService<ITerminalTextHandler>());
                    commandDescriptor.OptionDescriptors.RegisterHelp(helpDescriptor);
                }
            });
        }

        /// <summary>
        /// Stops the hosted service asynchronously.
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
            if (license.Plan == TerminalLicensePlans.Demo)
            {
                if (license.Usage == LicenseUsage.Educational)
                {
                    Logger.LogWarning("Your demo license is free for educational purposes. For non-educational, release, or production environment, you require a commercial license.");
                }
                else if (license.Usage == LicenseUsage.RnD)
                {
                    Logger.LogWarning("Your demo license is free for RnD, test, and evaluation purposes. For release, or production environment, you require a commercial license.");
                }
            }
            else if (license.Plan == TerminalLicensePlans.Custom)
            {
                if (license.Usage == LicenseUsage.RnD)
                {
                    Logger.LogWarning("Your custom license is free for RnD, test and evaluation purposes. For release, or production environment, you require a commercial license.");
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
        /// Triggered when the application host has fully started.
        /// </summary>
        protected virtual void OnStarted()
        {
            Logger.LogInformation("Application started on {0}.", DateTime.UtcNow.ToLocalTime().ToString());
            Logger.LogInformation("");
        }

        /// <summary>
        /// Triggered when the application host has completed a graceful shutdown. The application will
        /// not exit until all callbacks registered on this token have completed.
        /// </summary>
        protected virtual void OnStopped()
        {
            Logger.LogInformation("Application stopped on {0}.", DateTime.UtcNow.ToLocalTime().ToString());
        }

        /// <summary>
        /// Triggered when the application host is starting a graceful shutdown. Shutdown will block until
        /// all callbacks registered on this token have completed.
        /// </summary>
        protected virtual void OnStopping()
        {
            Logger.LogInformation("Stopping application...");
        }

        /// <summary>
        /// Allows the host application to print the custom header.
        /// </summary>
        protected abstract Task PrintHostApplicationHeaderAsync();

        /// <summary>
        /// Allows host application to print custom licensing information.
        /// </summary>
        /// <param name="license"></param>
        /// <returns></returns>
        protected virtual Task PrintHostApplicationLicensingAsync(License license)
        {
            // Print the license information
            Logger.LogInformation("tenant={0} ({1})", license.Claims.TenantName, license.Claims.TenantId);
            Logger.LogInformation("country={0}", license.Claims.TenantCountry);
            Logger.LogInformation("license={0}", license.Claims.Id);
            Logger.LogInformation("mode={0}", license.Claims.Mode);
            Logger.LogInformation("deployment={0}", license.Claims.Deployment);
            Logger.LogInformation("usage={0}", license.Usage);
            Logger.LogInformation("plan={0}", license.Plan);
            Logger.LogInformation("iat={0}", license.Claims.IssuedAt);
            Logger.LogInformation("exp={0}", license.Claims.ExpiryAt);

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
            IConfigurationOptionsChecker optionsChecker = ServiceProvider.GetRequiredService<IConfigurationOptionsChecker>();
            await optionsChecker.CheckAsync(options);
        }

        private async Task CheckLicenseAsync(LicenseExtractorResult result)
        {
            ILicenseChecker licenseChecker = ServiceProvider.GetRequiredService<ILicenseChecker>();
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(result.License));
        }

        private async Task<LicenseExtractorResult> ExtractLicenseAsync()
        {
            ILicenseExtractor licenseExtractor = ServiceProvider.GetRequiredService<ILicenseExtractor>();
            LicenseExtractorResult result = await licenseExtractor.ExtractLicenseAsync(new LicenseExtractorContext());
            return result;
        }

        /// <summary>
        /// The terminal configuration options.
        /// </summary>
        protected TerminalOptions Options { get; private set; }

        /// <summary>
        /// The host application lifetime.
        /// </summary>
        protected IHostApplicationLifetime HostApplicationLifetime { get; private set; }

        /// <summary>
        /// The logger.
        /// </summary>
        protected ILogger<TerminalHostedService> Logger { get; private set; }

        /// <summary>
        /// The service provider.
        /// </summary>
        protected IServiceProvider ServiceProvider { get; private set; }
    }
}