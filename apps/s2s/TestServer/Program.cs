using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Apps.TestServer.Runners;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Server;
using OneImlx.Terminal.Server.Extensions;
using OneImlx.Terminal.Shared;
using OneImlx.Terminal.Stores;
using Serilog;

namespace OneImlx.Terminal.Apps.TestServer
{
    /// <summary>
    /// This is a sample application that demonstrates the flexibility of the <c>OneImlx.Terminal</c> framework. The
    /// server showcases how to configure multi-setup for various network protocols like TCP, UDP, gRPC, and HTTP.
    /// Developers can use this sample to implement multi-setup network handling based on `appsettings.json`
    /// configuration or choose a specific protocol as per their application requirements. It also demonstrates how to
    /// structure services, logging, and terminal commands using OneImlx.Terminal capabilities. The flexibility to
    /// switch between protocols is a powerful feature that allows this framework to handle diverse environments.
    /// </summary>
    internal class Program
    {
        // Configure logging to use Serilog. Developers can extend or modify the logging configuration as per their needs.
        private static void ConfigureLoggingDelegate(ILoggingBuilder builder)
        {
            builder.ClearProviders();
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Error()
                .WriteTo.Console();
            Log.Logger = loggerConfig.CreateLogger();
            builder.AddSerilog(Log.Logger);
        }

        // Configure services based on the application settings. Developers can modify or add additional services if needed.
        private static void ConfigureServicesDelegate(IConfiguration configuration, IServiceCollection services)
        {
            services.Configure<ConsoleLifetimeOptions>(static options => options.SuppressStatusMessages = true);
            ConfigureTerminalServices(configuration, services);
        }

        // Core of the OneImlx.Terminal multi-protocol setup. This method demonstrates setting up different terminal
        // routers based on appsettings.
        private static void ConfigureTerminalServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddHostedService<TestServerHostedService>();

            // Terminal builder helps in setting up terminal-related services and configurations for different protocols.
            ITerminalBuilder terminalBuilder = services.AddTerminalConsole<TerminalInMemoryCommandStore, TerminalConsoleHelpProvider, TerminalConsoleExceptionHandler, TerminalSystemConsole>(
                new TerminalTextHandler(StringComparison.OrdinalIgnoreCase, Encoding.Unicode),
                options =>
                {
                    options.Id = TerminalIdentifiers.TestApplicationId; // Set the application ID.
                    options.Licensing.LicenseFile = "C:\\this\\lic\\oneimlx-terminal-demo-test.json"; // License file path.
                    options.Licensing.LicensePlan = TerminalLicensePlans.Demo; // License plan to use (Demo in this case).
                    options.Router.MaxLength = 64000; // Set max length for remote messages.
                    options.Router.Caret = "> "; // Caret for the terminal.
                    options.Router.DisableResponse = Convert.ToBoolean(configuration["testserver:disable_response"]); // Disable response for the terminal.
                });

            // Read the mode (protocol) from the configuration file.
            string? mode = configuration.GetValue<string>("testserver:mode");
            if (string.IsNullOrWhiteSpace(mode))
            {
                throw new InvalidOperationException("The 'testserver:mode' configuration value is missing or empty.");
            }

            // Set up the appropriate terminal router based on the mode.
            switch (mode.ToLowerInvariant())
            {
                case "user":
                    terminalBuilder.AddTerminalRouter<TerminalConsoleRouter, TerminalConsoleRouterContext>();
                    break;

                case "tcp":
                    terminalBuilder.AddTerminalRouter<TerminalTcpRouter, TerminalTcpRouterContext>();
                    break;

                case "udp":
                    terminalBuilder.AddTerminalRouter<TerminalUdpRouter, TerminalUdpRouterContext>();
                    break;

                case "grpc":
                    terminalBuilder.AddTerminalRouter<TerminalGrpcRouter, TerminalGrpcRouterContext>();
                    services.AddGrpc();
                    break;

                case "http":
                    services.AddScoped<TerminalHttpMapService>();
                    services.AddHttpClient();
                    terminalBuilder.AddTerminalRouter<TerminalHttpRouter, TerminalHttpRouterContext>();
                    break;

                default:
                    throw new InvalidOperationException($"The mode '{mode}' is not supported.");
            }

            terminalBuilder.AddDeclarativeAssembly<TestServerRunner>();
        }

        // Entry point for the application. This method reads the configuration and decides which protocol to initialize.
        private static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Read the mode from the configuration.
            string? mode = config.GetValue<string>("testserver:mode");
            if (string.IsNullOrWhiteSpace(mode))
            {
                throw new InvalidOperationException("The 'testserver:mode' configuration value is missing or empty.");
            }

            // Choose appropriate method based on the mode (gRPC, HTTP, TCP, etc.).
            if (mode.Equals("grpc", StringComparison.OrdinalIgnoreCase))
            {
                await RunWebApplicationGrpcAsync(args, config);
            }
            else if (mode.Equals("http", StringComparison.OrdinalIgnoreCase))
            {
                await RunWebApplicationHttpAsync(args, config);
            }
            else
            {
                await RunHostApplicationAsync(args, config);
            }

            await host!.WaitForShutdownAsync();
        }

        // Handles the execution of the console router for the terminal.
        private static async Task RunConsoleRouterAsync()
        {
            TerminalConsoleRouterContext routerContext = new(TerminalStartMode.Console, CancellationToken.None, CancellationToken.None);
            await host!.RunTerminalRouterAsync<TerminalConsoleRouter, TerminalConsoleRouterContext>(routerContext);
        }

        // Handles the execution of the gRPC router for the terminal.
        private static async Task RunGrpcRouterAsync()
        {
            TerminalGrpcRouterContext routerContext = new(TerminalStartMode.Grpc, CancellationToken.None, CancellationToken.None);
            await host!.RunTerminalRouterAsync<TerminalGrpcRouter, TerminalGrpcRouterContext>(routerContext);
        }

        // Runs the application host for non-gRPC, non-HTTP protocols (like TCP, UDP, etc.)
        private static async Task RunHostApplicationAsync(string[] args, IConfiguration configuration)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Configuration.AddConfiguration(configuration);
            ConfigureLoggingDelegate(builder.Logging);
            ConfigureServicesDelegate(configuration, builder.Services); // Setup services

            host = builder.Build();
            await host.StartAsync();
            await StartRouterAsync();
        }

        // Handles the execution of the HTTP router for the terminal.
        private static async Task RunHttpRouterAsync(IConfiguration configuration)
        {
            string ipAddress = configuration.GetValue<string>("testserver:ip")
                ?? throw new InvalidOperationException("The 'testserver:ip' configuration value is missing.");
            int port = configuration.GetValue<int?>("testserver:port")
                ?? throw new InvalidOperationException("The 'testserver:port' configuration value is missing or invalid.");

            IPAddress serverLocalIp = IPAddress.Parse(ipAddress);
            IPEndPoint iPEndPoint = new(serverLocalIp, port);
            TerminalHttpRouterContext routerContext = new(iPEndPoint, TerminalStartMode.Http, CancellationToken.None, CancellationToken.None);

            await host!.RunTerminalRouterAsync<TerminalHttpRouter, TerminalHttpRouterContext>(routerContext);
        }

        // Handles the execution of the TCP router for the terminal.
        private static async Task RunTcpRouterAsync(IConfiguration configuration)
        {
            string ipAddress = configuration.GetValue<string>("testserver:ip") ?? throw new InvalidOperationException("The 'testserver:ip' configuration value is missing.");
            int port = configuration.GetValue<int?>("testserver:port") ?? throw new InvalidOperationException("The 'testserver:port' configuration value is missing or invalid.");

            IPAddress serverLocalIp = IPAddress.Parse(ipAddress);
            IPEndPoint iPEndPoint = new(serverLocalIp, port);
            TerminalTcpRouterContext routerContext = new(iPEndPoint, TerminalStartMode.Tcp, CancellationToken.None, CancellationToken.None);
            await host!.RunTerminalRouterAsync<TerminalTcpRouter, TerminalTcpRouterContext>(routerContext);
        }

        // Handles the execution of the UDP router for the terminal.
        private static async Task RunUdpRouterAsync(IConfiguration configuration)
        {
            string ipAddress = configuration.GetValue<string>("testserver:ip") ?? throw new InvalidOperationException("The 'testserver:ip' configuration value is missing.");
            int port = configuration.GetValue<int?>("testserver:port") ?? throw new InvalidOperationException("The 'testserver:port' configuration value is missing or invalid.");

            IPAddress serverLocalIp = IPAddress.Parse(ipAddress);
            IPEndPoint iPEndPoint = new(serverLocalIp, port);
            TerminalUdpRouterContext routerContext = new(iPEndPoint, TerminalStartMode.Udp, CancellationToken.None, CancellationToken.None);
            await host!.RunTerminalRouterAsync<TerminalUdpRouter, TerminalUdpRouterContext>(routerContext);
        }

        // This method sets up a gRPC-based web application. Kestrel is configured to use HTTP/2.
        private static async Task RunWebApplicationGrpcAsync(string[] args, IConfiguration configuration)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddConfiguration(configuration);
            ConfigureLoggingDelegate(builder.Logging);

            string ipAddress = configuration.GetValue<string>("testserver:ip") ?? throw new InvalidOperationException("The 'testserver:ip' configuration value is missing or invalid.");
            int port = configuration.GetValue<int?>("testserver:port") ?? throw new InvalidOperationException("The 'testserver:port' configuration value is missing or invalid.");

            builder.WebHost.UseKestrel(options =>
            {
                options.Listen(IPAddress.Parse(ipAddress), port, listenOptions =>
                {
                    listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
                });
            });

            ConfigureServicesDelegate(builder.Configuration, builder.Services);

            var app = builder.Build();
            host = app;

            app.MapGrpcService<TerminalGrpcMapService>();

            await app.StartAsync();
            await StartRouterAsync();
        }

        // This method sets up an HTTP-based web application. Kestrel is configured to use HTTP/1.x.
        private static async Task RunWebApplicationHttpAsync(string[] args, IConfiguration configuration)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddConfiguration(configuration);
            ConfigureLoggingDelegate(builder.Logging);

            string ipAddress = configuration.GetValue<string>("testserver:ip") ?? throw new InvalidOperationException("The 'testserver:ip' configuration value is missing or invalid.");
            int port = configuration.GetValue<int?>("testserver:port") ?? throw new InvalidOperationException("The 'testserver:port' configuration value is missing or invalid.");

            builder.WebHost.UseKestrel(options =>
            {
                options.Listen(IPAddress.Parse(ipAddress), port, listenOptions =>
                {
                    listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
                });
            });

            ConfigureServicesDelegate(builder.Configuration, builder.Services);

            WebApplication app = builder.Build();
            host = app;

            // Map the HTTP service to the terminal commands endpoint
            app.MapTerminalHttp();

            await app.StartAsync();
            await StartRouterAsync();
        }

        // Determines the terminal router mode and initializes the correct router (Console, TCP, UDP, gRPC, or HTTP).
        private static async Task StartRouterAsync()
        {
            if (host == null)
            {
                throw new InvalidOperationException("Host is not initialized.");
            }

            var configuration = host.Services.GetRequiredService<IConfiguration>();
            string mode = configuration.GetValue<string>("testserver:mode") ?? throw new InvalidOperationException("The 'testserver:mode' configuration value is missing.");
            switch (mode.ToLowerInvariant())
            {
                case "user":
                    await RunConsoleRouterAsync();
                    break;

                case "tcp":
                    await RunTcpRouterAsync(configuration);
                    break;

                case "udp":
                    await RunUdpRouterAsync(configuration);
                    break;

                case "grpc":
                    await RunGrpcRouterAsync();
                    break;

                case "http":
                    await RunHttpRouterAsync(configuration);
                    break;

                default:
                    throw new InvalidOperationException($"The mode '{mode}' is not supported.");
            }
        }

        private static IHost? host;
    }
}
