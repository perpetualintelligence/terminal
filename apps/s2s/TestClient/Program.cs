/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Apps.Runners;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Stores;
using Serilog;

namespace OneImlx.Terminal.Apps.TestClient
{
    internal class Program
    {
        private static void ConfigureAppConfigurationDelegate(HostBuilderContext context, IConfigurationBuilder builder)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"), optional: false, reloadOnChange: false);
            configBuilder.Build();
        }

        private static void ConfigureLoggingDelegate(HostBuilderContext context, ILoggingBuilder builder)
        {
            // Clear all providers
            builder.ClearProviders();

            // Configure logging of your choice, here we are configuring Serilog
            var loggerConfig = new LoggerConfiguration();
            loggerConfig.MinimumLevel.Error();
            loggerConfig.WriteTo.Console();
            Log.Logger = loggerConfig.CreateLogger();
            builder.AddSerilog(Log.Logger);
        }

        private static void ConfigureOneImlxTerminal(HostBuilderContext context, IServiceCollection collection)
        {
            // Configure the hosted service
            collection.AddHostedService<TestClientHostedService>();

            // NOTE: We are initialized as a console application. This can be a custom console or a custom terminal
            // interface as well.
            ITerminalBuilder terminalBuilder = collection.AddTerminalConsole<TerminalInMemoryCommandStore, TerminalUnicodeTextHandler, TerminalHelpConsoleProvider, TerminalSystemConsole>(new TerminalUnicodeTextHandler(),
                options =>
                {
                    options.Id = TerminalIdentifiers.TestApplicationId;
                    options.Licensing.LicenseFile = "C:\\this\\lic\\oneimlx-terminal-demo-test.json";
                    options.Licensing.LicensePlan = TerminalLicensePlans.Demo;
                    options.Licensing.Deployment = TerminalIdentifiers.OnPremiseDeployment;

                    options.Router.Caret = "> ";
                }
                                                                                                                                                                                          );

            // Add commands using declarative syntax.
            terminalBuilder.AddDeclarativeAssembly<TestClientRunner>();
        }

        private static void ConfigureServicesDelegate(HostBuilderContext context, IServiceCollection services)
        {
            // Disable hosting status message
            services.Configure<ConsoleLifetimeOptions>(options =>
            {
                options.SuppressStatusMessages = true;
            });

            // Configure OneImlx.Terminal services
            ConfigureOneImlxTerminal(context, services);

            // Configure other services
        }

        private static async Task Main(string[] args)
        {
            // Allows cancellation for the entire terminal and individual commands.
            CancellationTokenSource terminalTokenSource = new();
            CancellationTokenSource commandTokenSource = new();

            // Setup the terminal start context
            TerminalStartContext terminalStartContext = new(TerminalStartMode.Console, terminalTokenSource.Token, commandTokenSource.Token);
            TerminalConsoleRouterContext terminalConsoleRouterContext = new(terminalStartContext);

            // Start the host so we can configure the terminal router.
            // NOTE: The host needs to be started with Start API to initialize the hosted service.
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(ConfigureAppConfigurationDelegate)
                .ConfigureLogging(ConfigureLoggingDelegate)
                .ConfigureServices(ConfigureServicesDelegate)
                .RunTerminalRouter<TerminalConsoleRouter, TerminalConsoleRouterContext>(terminalConsoleRouterContext);
        }

        private static IHost? host;
    }
}
