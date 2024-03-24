/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Stores;

namespace OneImlx.Terminal.Apps.TestApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // Allows cancellation for the terminal.
            CancellationTokenSource terminalCancellationTokenSource = new();
            CancellationTokenSource cancellationTokenSource = new();

            // Setup and start the host builder.
            // Note: The host should only start, the terminal framework will run the router separately
            IHostBuilder hostBuilder = CreateHostBuilder(args);
            hostBuilder.ConfigureServices(ConfigureServicesDelegate);
            hostBuilder.ConfigureLogging(ConfigureLoggingDelegate);
            IHost host = hostBuilder.Start();

            // Setup the terminal context and run the router indefinitely.
            TerminalStartContext terminalStartContext = new(TerminalStartMode.Console, terminalCancellationTokenSource.Token, cancellationTokenSource.Token);
            TerminalConsoleRouterContext consoleRouterContext = new(terminalStartContext);
            await host.RunTerminalRouterAsync<TerminalConsoleRouter, TerminalConsoleRouterContext>(consoleRouterContext);
        }

        private static void ConfigureLoggingDelegate(HostBuilderContext context, ILoggingBuilder builder)
        {
            // Clear all providers
            builder.ClearProviders();

            // Add Console
            builder.AddConsole();
        }

        private static void ConfigureServicesDelegate(HostBuilderContext context, IServiceCollection collection)
        {
            // OneImlx.Terminal
            ConfigureOneImlxTerminal(collection);

            // Configure other services
        }

        private static void ConfigureOneImlxTerminal(IServiceCollection collection)
        {
            // Configure the hosted service
            collection.AddHostedService<TestAppHostedService>();

            // We are using online license so configure Http
            collection.AddHttpClient("demo-http");

            // Specific your demo or commercial license file.
            ITerminalBuilder terminalBuilder = collection.AddTerminalConsole<TerminalInMemoryImmutableCommandStore, TerminalUnicodeTextHandler, TerminalHelpConsoleProvider, TerminalSystemConsole>(new TerminalUnicodeTextHandler(),
                options =>
                {
                    options.Id = TerminalIdentifiers.TestApplicationId;
                    options.Licensing.LicenseFile = "C:\\this\\perpetualintelligence\\tools\\lic\\oneimlx-terminal-demo-test.json";
                    options.Licensing.HttpClientName = "demo-http";
                }
            );
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