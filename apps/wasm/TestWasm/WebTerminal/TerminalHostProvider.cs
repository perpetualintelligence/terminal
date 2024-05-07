/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Hosting;
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
    public sealed class TerminalHostProvider
    {
        public TerminalHostProvider(ILogger<TerminalHostProvider> logger)
        {
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
        public async Task<IHost> StartTerminalHostAsync()
        {
            if (IsTerminalHostRunning)
            {
                throw new InvalidOperationException("The terminal host is already running.");
            }

            // Setup the terminal context and run the router indefinitely as a console.
            logger.LogInformation("Starting terminal host...");
            terminalTokenSource = new CancellationTokenSource();
            commandTokenSource = new CancellationTokenSource();
            TerminalStartContext terminalStartContext = new(TerminalStartMode.Console, terminalTokenSource.Token, commandTokenSource.Token);
            TerminalConsoleRouterContext consoleRouterContext = new(terminalStartContext);

            // Start the host builder and run terminal router till canceled.
            terminalHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(ConfigureAppConfigurationDelegate)
                .ConfigureLogging(ConfigureLoggingDelegate)
                .ConfigureServices(ConfigureServicesDelegate)
                .Build();

            logger.LogInformation("Running terminal router...");
            await terminalHost.RunTerminalRouterAsync<TerminalConsoleRouter, TerminalConsoleRouterContext>(consoleRouterContext);
            return terminalHost;
        }

        /// <summary>
        /// Configures application settings from JSON files and other configuration sources.
        /// </summary>
        private static void ConfigureAppConfigurationDelegate(HostBuilderContext context, IConfigurationBuilder builder)
        {
            builder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        }

        /// <summary>
        /// Sets up logging services using Serilog for detailed logging throughout the application.
        /// </summary>
        private static void ConfigureLoggingDelegate(HostBuilderContext context, ILoggingBuilder builder)
        {
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
        private static void ConfigureOneImlxTerminal(IServiceCollection services)
        {
            services.AddHostedService<TestWasmHostedService>();

            var terminalBuilder = services.AddTerminalConsole<TerminalInMemoryCommandStore, TerminalUnicodeTextHandler, TerminalHelpConsoleProvider, TerminalSystemConsole>(new TerminalUnicodeTextHandler(), options =>
            {
                options.Id = TerminalIdentifiers.TestApplicationId;
                options.Licensing.LicenseFile = "C:\\Users\\PerpetualAdmin\\source\\repos\\perpetualintelligence\\tools\\lic\\oneimlx-terminal-demo-test.json";
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
        private static void ConfigureServicesDelegate(HostBuilderContext context, IServiceCollection services)
        {
            ConfigureOneImlxTerminal(services);
        }

        private readonly ILogger<TerminalHostProvider> logger;
        private CancellationTokenSource? commandTokenSource;
        private IHost? terminalHost;
        private CancellationTokenSource? terminalTokenSource;
    }
}
