using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Apps.TestClient.Runners;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Stores;
using Serilog;

namespace OneImlx.Terminal.Apps.TestClient
{
    internal class Program
    {
        private static void ConfigureLoggingDelegate(ILoggingBuilder builder)
        {
            builder.ClearProviders();

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Error()
                .WriteTo.Console();
            Log.Logger = loggerConfig.CreateLogger();
            builder.AddSerilog(Log.Logger);
        }

        private static void ConfigureOneImlxTerminal(IConfiguration configuration, IServiceCollection services)
        {
            // Configure the hosted service
            services.AddHostedService<TestClientHostedService>();

            // Initialize the terminal as a console application
            ITerminalBuilder terminalBuilder = services.AddTerminalConsole<TerminalInMemoryCommandStore, TerminalConsoleHelpProvider, TerminalConsoleExceptionHandler, TerminalSystemConsole>(
                new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.ASCII),
                options =>
                {
                    options.Id = TerminalIdentifiers.TestApplicationId;
                    options.Licensing.LicenseFile = "C:\\this\\lic\\oneimlx-terminal-demo-test.json";
                    options.Licensing.LicensePlan = TerminalLicensePlans.Demo;
                    options.Licensing.Deployment = TerminalIdentifiers.OnPremiseDeployment;
                    options.Router.Caret = "> ";
                });

            // Add commands using declarative syntax.
            terminalBuilder.AddDeclarativeAssembly<TestClientRunner>();
        }

        private static void ConfigureServicesDelegate(IConfiguration configuration, IServiceCollection services)
        {
            // Disable hosting status message
            services.Configure<ConsoleLifetimeOptions>(static options =>
            {
                options.SuppressStatusMessages = true;
            });

            // Configure OneImlx.Terminal services
            ConfigureOneImlxTerminal(configuration, services);

            // Configure other services as needed
            services.AddHttpClient();
        }

        private static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Configuration.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"), optional: false, reloadOnChange: false);

            ConfigureLoggingDelegate(builder.Logging);
            ConfigureServicesDelegate(builder.Configuration, builder.Services);

            // Build and start the host
            var host = builder.Build();
            await host.StartAsync();

            // Allows cancellation for the entire terminal and individual commands.
            CancellationTokenSource terminalTokenSource = new();
            CancellationTokenSource commandTokenSource = new();

            // Setup the terminal start context
            TerminalStartContext terminalStartContext = new(TerminalStartMode.Console, terminalTokenSource.Token, commandTokenSource.Token);
            TerminalConsoleRouterContext terminalConsoleRouterContext = new(terminalStartContext);

            // Run the terminal router
            await host.RunTerminalRouterAsync<TerminalConsoleRouter, TerminalConsoleRouterContext>(terminalConsoleRouterContext);

            // Wait for the host to shut down
            await host.WaitForShutdownAsync();

            // Wait for user to acknowledge the shutdown
            Console.ReadLine();
        }
    }
}
