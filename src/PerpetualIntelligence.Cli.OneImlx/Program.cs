/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PerpetualIntelligence.Protocols.OneImlx;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Diagnostics;
using System.Reflection;

namespace PerpetualIntelligence.OneImlx.Cli
{
    /// <summary>
    /// Main program entry point.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main method.
        /// </summary>
        /// <param name="args"></param>
        public static Task Main(string[] args)
        {
            try
            {
                // Serilog setup
                InitSerilog();

                // Console headers
                WriteHeaders();

                // https://stackoverflow.com/questions/51357799/host-net-core-console-application-like-windows-service
                // Start the host. We don't call Run as it will block the thread. We want to listen to user inputs.
                using IHost host = CreateHostBuilder(args, Startup.ConfigureServices)
                    .UseSerilog()
                    .Start();
            }
            catch (Exception ex)
            {
                Log.Fatal("Unhandled fatal error. message={0}", ex.Message);
            }
            finally
            {
                Log.CloseAndFlush();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Creates a host builder.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <param name="configureDelegate"></param>
        /// <returns></returns>
        private static IHostBuilder CreateHostBuilder(string[] args, Action<IServiceCollection> configureDelegate)
        {
            // https://www.programmingwithwolfgang.com/configure-dependency-injection-for-net-5-console-applications/
            return Host.CreateDefaultBuilder(args).ConfigureServices(configureDelegate);
        }

        private static void InitSerilog()
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;

            Console.Title = OrgConstants.FullName;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Error)
                .MinimumLevel.Override("System", LogEventLevel.Error)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Error)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:w4}] {Message:lj}{NewLine}{Exception}", theme: AnsiConsoleTheme.Code)
                .CreateLogger();
        }

        private static void WriteHeaders()
        {
            Console.WriteLine("---------------------------------------------------------------------------------------------");
            Console.WriteLine("Copyright (c). All Rights Reserved. Perpetual Intelligence L.L.C.");
            Console.WriteLine("https://perpetualintelligence.com");
            Console.WriteLine("https://api.perpetualintelligence.com");
            Console.WriteLine("https://oneimlx.com");
            Console.WriteLine("---------------------------------------------------------------------------------------------");

            Console.WriteLine($"Starting server...");
            Thread.Sleep(500);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"urn:oneimlx:cli version={typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? " < none > "}");
            Console.ResetColor();
        }
    }
}
