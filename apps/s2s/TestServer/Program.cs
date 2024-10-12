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
using OneImlx.Terminal.Stores;
using Serilog;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.TestServer
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

        private static void ConfigureServicesDelegate(IConfiguration configuration, IServiceCollection services)
        {
            services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);
            ConfigureTerminalServices(configuration, services);
        }

        private static void ConfigureTerminalServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddHostedService<TestServerHostedService>();

            ITerminalBuilder terminalBuilder = services.AddTerminalConsole<TerminalInMemoryCommandStore, TerminalUnicodeTextHandler, TerminalConsoleHelpProvider, TerminalConsoleExceptionHandler, TerminalSystemConsole>(
                new TerminalUnicodeTextHandler(),
                options =>
                {
                    options.Id = TerminalIdentifiers.TestApplicationId;
                    options.Licensing.LicenseFile = "C:\\this\\lic\\oneimlx-terminal-demo-test.json";
                    options.Licensing.LicensePlan = TerminalLicensePlans.Demo;
                    options.Licensing.Deployment = TerminalIdentifiers.OnPremiseDeployment;
                    options.Router.RemoteMessageMaxLength = 64000;
                    options.Router.EnableRemoteDelimiters = true;
                    options.Router.Caret = "> ";
                });

            string? mode = configuration.GetValue<string>("testserver:mode");
            if (string.IsNullOrWhiteSpace(mode))
            {
                throw new InvalidOperationException("The 'testserver:mode' configuration value is missing or empty.");
            }

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
                    services.AddGrpcReflection();
                    break;

                default:
                    throw new InvalidOperationException($"The mode '{mode}' is not supported.");
            }

            // Add the command declarative assembly
            terminalBuilder.AddDeclarativeAssembly<TestServerRunner>();
        }

        private static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string? mode = config.GetValue<string>("testserver:mode");
            if (string.IsNullOrWhiteSpace(mode))
            {
                throw new InvalidOperationException("The 'testserver:mode' configuration value is missing or empty.");
            }

            if (mode.Equals("grpc", StringComparison.OrdinalIgnoreCase))
            {
                await RunWebApplicationAsync(args, config);
            }
            else
            {
                await RunHostApplicationAsync(args, config);
            }

            await host!.WaitForShutdownAsync();
        }

        private static async Task RunConsoleRouterAsync()
        {
            TerminalStartContext startContext = new(TerminalStartMode.Console, CancellationToken.None, CancellationToken.None);
            TerminalConsoleRouterContext routerContext = new(startContext);
            await host!.RunTerminalRouterAsync<TerminalConsoleRouter, TerminalConsoleRouterContext>(routerContext);
        }

        private static async Task RunGrpcRouterAsync()
        {
            TerminalStartContext startContext = new(TerminalStartMode.Grpc, CancellationToken.None, CancellationToken.None);
            TerminalGrpcRouterContext routerContext = new(startContext);
            await host!.RunTerminalRouterAsync<TerminalGrpcRouter, TerminalGrpcRouterContext>(routerContext);
        }

        private static async Task RunHostApplicationAsync(string[] args, IConfiguration configuration)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Configuration.AddConfiguration(configuration);
            ConfigureLoggingDelegate(builder.Logging);
            ConfigureServicesDelegate(configuration, builder.Services);

            host = builder.Build();
            await host.StartAsync();
            await StartRouterAsync();
        }

        private static async Task RunTcpRouterAsync(IConfiguration configuration)
        {
            string ipAddress = configuration.GetValue<string>("testserver:ip") ?? throw new InvalidOperationException("The 'testserver:ip' configuration value is missing.");
            int port = configuration.GetValue<int?>("testserver:port") ?? throw new InvalidOperationException("The 'testserver:port' configuration value is missing or invalid.");

            IPAddress serverLocalIp = IPAddress.Parse(ipAddress);
            IPEndPoint iPEndPoint = new(serverLocalIp, port);
            TerminalStartContext startContext = new(TerminalStartMode.Tcp, CancellationToken.None, CancellationToken.None);
            TerminalTcpRouterContext routerContext = new(iPEndPoint, startContext);
            await host!.RunTerminalRouterAsync<TerminalTcpRouter, TerminalTcpRouterContext>(routerContext);
        }

        private static async Task RunUdpRouterAsync(IConfiguration configuration)
        {
            string ipAddress = configuration.GetValue<string>("testserver:ip") ?? throw new InvalidOperationException("The 'testserver:ip' configuration value is missing.");
            int port = configuration.GetValue<int?>("testserver:port") ?? throw new InvalidOperationException("The 'testserver:port' configuration value is missing or invalid.");

            IPAddress serverLocalIp = IPAddress.Parse(ipAddress);
            IPEndPoint iPEndPoint = new(serverLocalIp, port);
            TerminalStartContext startContext = new(TerminalStartMode.Udp, CancellationToken.None, CancellationToken.None);
            TerminalUdpRouterContext routerContext = new(iPEndPoint, startContext);
            await host!.RunTerminalRouterAsync<TerminalUdpRouter, TerminalUdpRouterContext>(routerContext);
        }

        private static async Task RunWebApplicationAsync(string[] args, IConfiguration configuration)
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
            app.MapGrpcReflectionService();

            var grpcTask = app.StartAsync();
            await StartRouterAsync();
            await grpcTask;
        }

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

                default:
                    throw new InvalidOperationException($"The mode '{mode}' is not supported.");
            }
        }

        private static IHost? host;
    }
}
