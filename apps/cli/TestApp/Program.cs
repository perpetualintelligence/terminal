using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Apps.TestApp.Runners;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Stores;
using Serilog;
using System;
using System.IO;
using System.Threading;

namespace OneImlx.Terminal.Apps.TestApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Allows cancellation for the entire terminal and individual commands.
            CancellationTokenSource terminalTokenSource = new();
            CancellationTokenSource commandTokenSource = new();

            // Setup the terminal context and run the router indefinitely as a console.
            TerminalStartContext terminalStartContext = new(TerminalStartMode.Console, terminalTokenSource.Token, commandTokenSource.Token);
            TerminalConsoleRouterContext consoleRouterContext = new(terminalStartContext);

            // Start the host builder and run terminal router till canceled.
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(ConfigureAppConfigurationDelegate)
                .ConfigureLogging(ConfigureLoggingDelegate)
                .ConfigureServices(ConfigureServicesDelegate)
                .RunTerminalRouter<TerminalConsoleRouter, TerminalConsoleRouterContext>(consoleRouterContext);
        }

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

            // We are using on-line license so configure HTTP
            collection.AddHttpClient("demo-http");

            // NOTE:
            // Specify your demo or commercial license file.
            // Specify your application id.
            ITerminalBuilder terminalBuilder = collection.AddTerminalConsole<TerminalInMemoryCommandStore, TerminalUnicodeTextHandler, TerminalConsoleHelpProvider, TerminalConsoleExceptionHandler, TerminalSystemConsole>(new TerminalUnicodeTextHandler(),
                options =>
                {
                    options.Id = TerminalIdentifiers.TestApplicationId;
                    options.Licensing.LicenseFile = "C:\\Users\\PerpetualAdmin\\source\\repos\\perpetualintelligence\\tools\\lic\\oneimlx-terminal-demo-test.json";
                    options.Licensing.LicensePlan = TerminalLicensePlans.Demo;
                    options.Licensing.Deployment = TerminalIdentifiers.OnPremiseDeployment;
                    options.Router.Caret = "> ";
                }
            );

            // You can use declarative or explicit syntax. Here we are using declarative syntax.
            {
                // Add commands using declarative syntax.
                terminalBuilder.AddDeclarativeAssembly<TestRunner>();

                // OR

                // Add commands using explicit syntax.
                //RegisterCommands(terminalBuilder);
            }
        }

        private static void RegisterCommands(ITerminalBuilder terminalBuilder)
        {
            // Root Command
            terminalBuilder.DefineCommand<TestRunner>("test", "Test command", "Test Description", Commands.CommandType.Root, Commands.CommandFlags.None)
                                .DefineArgument(1, "arg1", nameof(String), "The first argument", Commands.ArgumentFlags.None)
                                    .Add()
                                .DefineArgument(2, "arg2", nameof(Int32), "The second argument", Commands.ArgumentFlags.None)
                                    .Add()
                                .DefineOption("version", nameof(String), "The version option", Commands.OptionFlags.None, "v")
                                    .Add()
                           .Add();

            // Grp1 Command
            terminalBuilder.DefineCommand<Grp1Runner>("grp1", "Test Group1", "Test Group1 Description", Commands.CommandType.Group, Commands.CommandFlags.None)
                           .Owners(new Commands.OwnerIdCollection("test"))
                           .Add();

            // Cmd1 Command
            terminalBuilder.DefineCommand<Cmd1Runner>("cmd1", "Test Command1", "Test Command2 Description", Commands.CommandType.SubCommand, Commands.CommandFlags.None)
                           .Owners(new Commands.OwnerIdCollection("grp1"))
                           .Add();

            // Grp2 Command
            terminalBuilder.DefineCommand<Grp2Runner>("grp2", "Test Group2", "Test Group1 Description", Commands.CommandType.Group, Commands.CommandFlags.None)
                           .Owners(new Commands.OwnerIdCollection("grp1"))
                           .Add();

            // Cmd2 Command
            terminalBuilder.DefineCommand<Cmd2Runner>("cmd2", "Test Command2", "Test Command2 Description", Commands.CommandType.SubCommand, Commands.CommandFlags.None)
                           .Owners(new Commands.OwnerIdCollection("grp2"))
                           .Add();
        }
    }
}