using System;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Apps.TestAuth.Runners;
using OneImlx.Terminal.Authentication.Extensions;
using OneImlx.Terminal.Authentication.Msal;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Stores;
using Serilog;

namespace OneImlx.Terminal.Apps.TestAuth
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

        private static void ConfigureOneImlxTerminal(IServiceCollection collection)
        {
            // Configure the hosted service
            collection.AddHostedService<TestAuthHostedService>();

            // We are using on-line license so configure HTTP
            collection.AddHttpClient("demo-http").AddHttpMessageHandler<TestAuthDelegatingHandler>();

            // Before we configure the terminal, we need to configure the public client from MSAL. This is required for
            // the terminal to authenticate with Azure AD.
            // NOTE: Replace the client id with your own Microsoft Azure AD or Entra client id.
            IPublicClientApplication publicClientApplication = PublicClientApplicationBuilder.Create("83843fef-5480-42c1-8575-1936f64339b9")
                .WithAuthority(AzureCloudInstance.AzurePublic, "common") // Use the common authority
                .WithRedirectUri("http://localhost")
                .Build();

            // NOTE: Specify your demo or commercial license file. Specify your application id.
            TerminalTextHandler textHandler = new(StringComparison.OrdinalIgnoreCase, Encoding.Unicode);
            ITerminalBuilder terminalBuilder = collection.AddTerminalConsole<TerminalInMemoryCommandStore, TerminalConsoleHelpProvider, TerminalConsoleExceptionHandler, TerminalSystemConsole>(textHandler,
                options =>
                {
                    options.Authentication.Enabled = true;
                    options.Authentication.DefaultScopes = ["email", "profile", "openid"];

                    options.Id = TerminalIdentifiers.TestApplicationId;
                    options.Licensing.LicenseFile = "C:\\this\\lic\\oneimlx-terminal-demo-test.json";
                    options.Licensing.LicensePlan = TerminalLicensePlans.Demo;
                    options.Router.Caret = "> ";
                });

            // Add terminal authentication
            terminalBuilder.AddMsalAuthentication<MsalKiotaAuthProvider, MsalKiotaAuthProvider, TestAuthDelegatingHandler>(publicClientApplication);

            // You can use declarative or explicit syntax. Here we are using declarative syntax.
            {
                // Add commands using declarative syntax.
                terminalBuilder.AddDeclarativeAssembly<TestRunner>();
            }
        }

        private static void ConfigureServicesDelegate(HostBuilderContext context, IServiceCollection services)
        {
            // Disable hosting status message
            services.Configure<ConsoleLifetimeOptions>(static options =>
            {
                options.SuppressStatusMessages = true;
            });

            // Configure OneImlx.Terminal services
            ConfigureOneImlxTerminal(services);

            // Configure other services
        }

        private static void Main(string[] args)
        {
            // Allows cancellation for the entire terminal and individual commands.
            CancellationTokenSource terminalTokenSource = new();
            CancellationTokenSource commandTokenSource = new();

            // Setup the terminal context and run the router indefinitely as a console.
            TerminalConsoleRouterContext consoleRouterContext = new(TerminalStartMode.Console, terminalTokenSource.Token, commandTokenSource.Token);

            // Start the host builder and run terminal router till canceled.
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(ConfigureAppConfigurationDelegate)
                .ConfigureLogging(ConfigureLoggingDelegate)
                .ConfigureServices(ConfigureServicesDelegate)
                .RunTerminalRouter<TerminalConsoleRouter, TerminalConsoleRouterContext>(consoleRouterContext);
        }
    }
}
