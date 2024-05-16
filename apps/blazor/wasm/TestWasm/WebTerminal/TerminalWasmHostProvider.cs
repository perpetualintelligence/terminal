/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Hosting;
using OneImlx.Shared.Extensions;
using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Apps.TestWasm.WebTerminal.Runners;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Stores;
using Serilog;

namespace OneImlx.Terminal.Apps.TestWasm.WebTerminal
{
    /// <summary>
    /// Manages the life-cycle and configuration of a terminal host. Provides functionality to start and retrieve the
    /// terminal host, ensuring that it can only be started once.
    /// </summary>
    public sealed class TerminalWasmHostProvider
    {
        public TerminalWasmHostProvider(IHttpClientFactory httpClientFactory, ILogger<TerminalWasmHostProvider> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
        }

        /// <summary>
        /// Indicates whether the terminal host is currently running. Provides a quick check to verify the operation
        /// state of the terminal.
        /// </summary>
        public bool IsTerminalHostRunning => terminalHost != null;

        /// <summary>
        /// Gets the terminal console.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ITerminalConsole GetTerminalConsole()
        {
            if (!IsTerminalHostRunning)
            {
                throw new InvalidOperationException("The terminal host is not running.");
            }

            return terminalHost!.Services.GetRequiredService<ITerminalConsole>();
        }

        /// <summary>
        /// Retrieves the running terminal host instance. Throws an exception if the host is not running, ensuring safe
        /// access to a valid host instance.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the terminal host is not running.</exception>
        public IHost GetTerminalHost()
        {
            if (!IsTerminalHostRunning)
            {
                throw new InvalidOperationException("The terminal host is not running.");
            }

            return terminalHost!;
        }

        /// <summary>
        /// Initializes and starts the terminal host asynchronously. Ensures that the host is only started if it is not
        /// already running, preventing multiple instances.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the terminal host is already running.</exception>
        public async Task StartTerminalHostAsync()
        {
            if (IsTerminalHostRunning)
            {
                throw new InvalidOperationException("The terminal host is already running.");
            }

            logger.LogInformation("Preparing to start terminal host...");

            try
            {
                terminalTokenSource = new CancellationTokenSource();
                commandTokenSource = new CancellationTokenSource();
                TerminalStartContext terminalStartContext = new(TerminalStartMode.Console, terminalTokenSource.Token, commandTokenSource.Token);
                TerminalConsoleRouterContext consoleRouterContext = new(terminalStartContext);

                // Get the license asynchronously so we can initialize the terminal host
                licenseContents = await GetLicenseContentAsync();

                // Create the terminal host
                terminalHost = Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration(ConfigureAppConfigurationDelegate)
                    .ConfigureLogging(ConfigureLoggingDelegate)
                    .ConfigureServices(ConfigureServicesDelegate)
                    .Build();

                logger.LogInformation("Terminal host built, starting now...");
                await terminalHost.StartAsync();

                logger.LogInformation("Terminal host started, now starting terminal router...");
                await terminalHost.RunTerminalRouterAsync<TerminalConsoleRouter, TerminalConsoleRouterContext>(consoleRouterContext);

                logger.LogInformation("Terminal router is now running.");
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while starting the terminal host: {ex}");
                throw; // Re-throwing the exception to be handled further up the call stack if necessary.
            }
        }


        /// <summary>
        /// Configures application settings from JSON files and other configuration sources.
        /// </summary>
        private void ConfigureAppConfigurationDelegate(HostBuilderContext context, IConfigurationBuilder builder)
        {
            builder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        }

        /// <summary>
        /// Sets up logging services using Serilog for detailed logging throughout the application.
        /// </summary>
        private void ConfigureLoggingDelegate(HostBuilderContext context, ILoggingBuilder builder)
        {
            builder.ClearProviders();

            var loggerConfig = new LoggerConfiguration()
                               .MinimumLevel.Debug()
                               .WriteTo.Console();
            Log.Logger = loggerConfig.CreateLogger();
            builder.AddSerilog(Log.Logger);
        }

        /// <summary>
        /// Configures and adds terminal-related services to the service collection, setting up command stores,
        /// handlers, and licensing details.
        /// </summary>
        private void ConfigureOneImlxTerminal(HostBuilderContext context, IServiceCollection services)
        {
            services.AddHostedService<TestWasmHostedService>();

            var terminalBuilder = services.AddTerminalConsole<TerminalInMemoryCommandStore, TerminalUnicodeTextHandler, TerminalHelpConsoleProvider, TerminalWasmConsole>(new TerminalUnicodeTextHandler(), options =>
            {
                options.Id = TerminalIdentifiers.TestApplicationId;
                options.Licensing.LicenseFile = "oneimlx-license.json";
                options.Licensing.LicenseContents = TerminalServices.EncodeLicenseContents(licenseContents.NotNull());
                options.Licensing.LicensePlan = TerminalLicensePlans.Demo;
                options.Licensing.Deployment = TerminalIdentifiers.OnPremiseDeployment;
                options.Router.Caret = "> ";
            });

            terminalBuilder.AddDeclarativeAssembly<TestRunner>();
        }

        /// <summary>
        /// Registers services and configurations specific to the terminal operations. This includes the terminal host
        /// service and any terminal-specific configurations and handlers.
        /// </summary>
        private void ConfigureServicesDelegate(HostBuilderContext context, IServiceCollection services)
        {
            // Add your services

            // Configure the terminal services
            ConfigureOneImlxTerminal(context, services);
        }

        private async Task<string> GetLicenseContentAsync()
        {
            HttpClient httpClient = httpClientFactory.CreateClient("base");
            string licenseContent = await httpClient.GetStringAsync("oneimlx-license.json");
            return licenseContent;
        }

        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<TerminalWasmHostProvider> logger;
        private CancellationTokenSource? commandTokenSource;
        private string? licenseContents;
        private IHost? terminalHost;
        private CancellationTokenSource? terminalTokenSource;
    }
}
