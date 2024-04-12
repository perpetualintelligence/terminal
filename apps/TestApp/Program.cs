/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Apps.TestApp.Runners;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Stores;
using Serilog;

namespace OneImlx.Terminal.Apps.TestApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // Allows cancellation for the entire terminal and individual commands.
            CancellationTokenSource terminalTokenSource = new();
            CancellationTokenSource commandTokenSource = new();

            // Setup and start the host builder.
            // Note: The host should only start, the terminal framework will run the router separately
            IHostBuilder hostBuilder = CreateHostBuilder(args);
            hostBuilder.ConfigureServices(ConfigureServicesDelegate);
            hostBuilder.ConfigureLogging(ConfigureLoggingDelegate);
            IHost host = hostBuilder.Start();

            // Setup the terminal context and run the router indefinitely.
            TerminalStartContext terminalStartContext = new(TerminalStartMode.Console, terminalTokenSource.Token, commandTokenSource.Token);
            TerminalConsoleRouterContext consoleRouterContext = new(terminalStartContext);
            await host.RunTerminalRouterAsync<TerminalConsoleRouter, TerminalConsoleRouterContext>(consoleRouterContext);
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

        private static void ConfigureServicesDelegate(HostBuilderContext context, IServiceCollection services)
        {
            // Disable hosting status message
            services.Configure<ConsoleLifetimeOptions>(options =>
            {
                options.SuppressStatusMessages = true;
            });

            // Configure OneImlx.Terminal services
            ConfigureOneImlxTerminal(services);

            // Configure other services
        }

        private static void ConfigureOneImlxTerminal(IServiceCollection collection)
        {
            // Configure the hosted service
            collection.AddHostedService<TestAppHostedService>();

            // We are using online license so configure HTTP
            collection.AddHttpClient("demo-http");

            // NOTE:
            // Specify your demo or commercial license file.
            // Specify your application id.
            ITerminalBuilder terminalBuilder = collection.AddTerminalConsole<TerminalInMemoryCommandStore, TerminalUnicodeTextHandler, TerminalHelpConsoleProvider, TerminalSystemConsole>(new TerminalUnicodeTextHandler(),
                options =>
                {
                    options.Id = TerminalIdentifiers.TestApplicationId;
                    options.Licensing.LicenseFile = "C:\\this\\perpetualintelligence\\tools\\lic\\oneimlx-terminal-demo-test.json";
                    options.Router.Caret = "> ";
                }
            );

            // Add commands using declarative syntax.
            terminalBuilder.AddDeclarativeAssembly<TestRunner>();
        }

        /// <summary>
        /// https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host?tabs=appbuilder
        /// </summary>
        /// <param name="args"></param>
        /// <param name="configureServices"></param>
        /// <returns></returns>
        private static IHostBuilder CreateHostBuilder(string[]? args)
        {
            return Host.CreateDefaultBuilder(args);
        }
    }
}