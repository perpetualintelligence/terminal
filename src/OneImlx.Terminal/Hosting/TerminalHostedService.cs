/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Stores;

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
        /// <param name="terminalOptions">The configuration options.</param>
        /// <param name="terminalConsole">The terminal console.</param>
        /// <param name="terminalExceptionHandler">The exception handler.</param>
        /// <param name="logger">The logger.</param>
        public TerminalHostedService(
            IServiceProvider serviceProvider,
            IOptions<TerminalOptions> terminalOptions,
            ITerminalConsole terminalConsole,
            ITerminalExceptionHandler terminalExceptionHandler,
            ILogger<TerminalHostedService> logger)
        {
            ServiceProvider = serviceProvider;
            Options = terminalOptions;
            TerminalConsole = terminalConsole;
            this.terminalExceptionHandler = terminalExceptionHandler;
            this.logger = logger;
        }

        /// <summary>
        /// Starts the hosted service asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // We catch the exception to avoid unhandled fatal exception
            try
            {
                // Configure the application lifetime.
                await ConfigureLifetimeAsync();

                // Do mandatory configuration check
                await CheckConfigurationAsync(Options);

                // Do license check
                await CheckLicenseAsync();

                // Register help options with command descriptors. Retrieving all commands from the store is
                // intentionally done at the end to avoid a performance hit in case the license check fails.

                await RegisterHelpAsync();
            }
            catch (TerminalException ex)
            {
                await terminalExceptionHandler.HandleExceptionAsync(new TerminalExceptionHandlerContext(ex));
            }
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
        /// The terminal configuration options.
        /// </summary>
        protected IOptions<TerminalOptions> Options { get; private set; }

        /// <summary>
        /// The service provider.
        /// </summary>
        protected IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// The terminal console.
        /// </summary>
        protected ITerminalConsole TerminalConsole { get; }

        /// <summary>
        /// Configures the application's lifetime with <see cref="IHostApplicationLifetime"/>.
        /// </summary>
        protected abstract Task ConfigureLifetimeAsync();

        /// <summary>
        /// This method check the mandatory configuration options. Applications cannot customize or change the mandatory
        /// configuration options, but they can perform additional configuration checks with <see cref="CheckConfigurationAsync(IOptions{TerminalOptions})"/>.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <returns></returns>
        private async Task CheckConfigurationAsync(IOptions<TerminalOptions> options)
        {
            IConfigurationOptionsChecker optionsChecker = ServiceProvider.GetRequiredService<IConfigurationOptionsChecker>();
            await optionsChecker.CheckAsync(options.Value);
        }

        private async Task CheckLicenseAsync()
        {
            ILicenseExtractor licenseExtractor = ServiceProvider.GetRequiredService<ILicenseExtractor>();
            LicenseExtractorResult exResult = await licenseExtractor.ExtractLicenseAsync();

            ILicenseChecker licenseChecker = ServiceProvider.GetRequiredService<ILicenseChecker>();
            LicenseCheckerResult chResult = await licenseChecker.CheckLicenseAsync(exResult.License);

            await PrintWarningIfDemoAsync(exResult.License);
        }

        /// <summary>
        /// This method prints the demo warning licensing details.
        /// </summary>
        /// <param name="license">The extracted license.</param>
        private async Task PrintWarningIfDemoAsync(Licensing.License license)
        {
            if (license.Plan == TerminalLicensePlans.Demo)
            {
                if (license.Usage == LicenseUsage.Educational)
                {
                    await TerminalConsole.WriteLineColorAsync(ConsoleColor.Yellow, "The demo license is free for educational purposes, but non-educational use requires a commercial license.");
                }
                else if (license.Usage == LicenseUsage.RnD)
                {
                    await TerminalConsole.WriteLineColorAsync(ConsoleColor.Yellow, "The demo license is free for research and development, but production use requires a commercial license.");
                }
            }
        }

        /// <summary>
        /// Registers the help options based on configuration options.
        /// </summary>
        /// <returns></returns>
        private async Task RegisterHelpAsync()
        {
            HelpOptions helpOptions = Options.Value.Help;

            if (!helpOptions.Enabled)
            {
                return;
            }

            // This can be a long list of command, but it is executed only once during startup.
            ITerminalCommandStore terminalCommandStore = ServiceProvider.GetRequiredService<ITerminalCommandStore>();
            var commandDescriptors = await terminalCommandStore.AllAsync();
            foreach (CommandDescriptor commandDescriptor in commandDescriptors.Values)
            {
                OptionDescriptor helpDescriptor = new(helpOptions.OptionId, nameof(Boolean), helpOptions.OptionDescription, OptionFlags.None, helpOptions.OptionAlias);

                commandDescriptor.OptionDescriptors ??= new OptionDescriptors(ServiceProvider.GetRequiredService<ITerminalTextHandler>());
                commandDescriptor.OptionDescriptors.RegisterHelp(helpDescriptor);
            }
        }

        private readonly ILogger<TerminalHostedService> logger;
        private readonly ITerminalExceptionHandler terminalExceptionHandler;
    }
}
