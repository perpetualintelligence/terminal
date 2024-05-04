/*
    Copyright (c) 2024 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Apps.TestServer.Runners;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Stores;
using Serilog;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.TestServer
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
            collection.AddHostedService<TestServerHostedService>();

            // NOTE: We are initialized as a console application. This can be a custom console or a custom terminal
            // interface as well.
            ITerminalBuilder terminalBuilder = collection.AddTerminalConsole<TerminalInMemoryCommandStore, TerminalUnicodeTextHandler, TerminalHelpConsoleProvider, TerminalSystemConsole>(new TerminalUnicodeTextHandler(),
                options =>
                {
                    options.Id = TerminalIdentifiers.TestApplicationId;
                    options.Licensing.LicenseFile = "C:\\this\\lic\\oneimlx-terminal-demo-test.json";
                    options.Licensing.LicensePlan = TerminalLicensePlans.Demo;
                    options.Licensing.Deployment = TerminalIdentifiers.OnPremiseDeployment;

                    options.Router.RemoteMessageMaxLength = 64000;
                    options.Router.EnableRemoteDelimiters = true;
                    options.Router.Caret = "> ";
                }
            );

            // Add router based on appsettings.json.
            string? mode = context.Configuration["testserver:mode"];
            if (mode == "user")
            {
                terminalBuilder.AddTerminalRouter<TerminalConsoleRouter, TerminalConsoleRouterContext>();
            }
            else if (mode == "tcp")
            {
                terminalBuilder.AddTerminalRouter<TerminalTcpRouter, TerminalTcpRouterContext>();
            }
            else if (mode == "udp")
            {
                terminalBuilder.AddTerminalRouter<TerminalUdpRouter, TerminalUdpRouterContext>();
            }
            else
            {
                throw new InvalidOperationException($"The mode `{mode}` is not supported.");
            }

            // Add commands using declarative syntax.
            terminalBuilder.AddDeclarativeAssembly<TestServerRunner>();
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

            // Start the host so we can configure the terminal router.
            // NOTE: The host needs to be started with Start API to initialize the hosted service.
            host = Host.CreateDefaultBuilder(args)
                       .ConfigureAppConfiguration(ConfigureAppConfigurationDelegate)
                       .ConfigureLogging(ConfigureLoggingDelegate)
                       .ConfigureServices(ConfigureServicesDelegate)
                       .Start();

            using (host)
            {
                // Get the mode so we can start the router in a user mode or a service mode.
                IConfiguration configuration = host.Services.GetRequiredService<IConfiguration>();
                string? mode = configuration["testserver:mode"];

                // Now configure router based on configuration
                if (mode == "user")
                {
                    TerminalStartContext startContext = new(TerminalStartMode.Console, terminalTokenSource.Token, commandTokenSource.Token);
                    TerminalConsoleRouterContext routerContext = new(startContext);
                    await host.RunTerminalRouterAsync<TerminalConsoleRouter, TerminalConsoleRouterContext>(routerContext);
                }
                else if (mode == "tcp")
                {
                    // Adjust this to your white-listed IP address needs.
                    string? ipAddress = configuration.GetValue<string>("testserver:ip");
                    int port = configuration.GetValue<int>("testserver:port");
                    IPAddress serverLocalIp = IPAddress.Parse(ipAddress!);
                    IPEndPoint iPEndPoint = new(serverLocalIp, port);
                    TerminalStartContext startContext = new(TerminalStartMode.Tcp, terminalTokenSource.Token, commandTokenSource.Token);

                    TerminalTcpRouterContext routerContext = new(iPEndPoint, startContext);
                    await host.RunTerminalRouterAsync<TerminalTcpRouter, TerminalTcpRouterContext>(routerContext);
                }
                else if (mode == "udp")
                {
                    // Adjust this to your white-listed IP address needs.
                    string? ipAddress = configuration.GetValue<string>("testserver:ip");
                    int port = configuration.GetValue<int>("testserver:port");
                    IPAddress serverLocalIp = IPAddress.Parse(ipAddress!);
                    IPEndPoint iPEndPoint = new(serverLocalIp, port);
                    TerminalStartContext startContext = new(TerminalStartMode.Udp, terminalTokenSource.Token, commandTokenSource.Token);

                    TerminalUdpRouterContext routerContext = new(iPEndPoint, startContext);
                    await host.RunTerminalRouterAsync<TerminalUdpRouter, TerminalUdpRouterContext>(routerContext);
                }
                else
                {
                    throw new InvalidOperationException($"The mode `{mode}` is not supported.");
                }
            }
        }

        private static IHost? host;
    }
}
