/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Extensions;
using PerpetualIntelligence.Protocols.OneImlx;
using PerpetualIntelligence.Shared.Extensions;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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
        public static async Task Main(string[] args)
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

                // The console loop for routing commands.
                await host.RunRoutingAsync("cmd > ", 5000, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Log.Fatal("Unhandled fatal error. message={0}", ex.Message);
            }
            finally
            {
                Log.CloseAndFlush();
            }
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

        private static async Task ExecuteExitCommandAsync(Command command)
        {
            string? answer = await ReadAnswerAsync("Are you sure ?", new[] { "y", "n" });
            if (answer == null || answer == "y")
            {
                //WriteLineRed("Shutting down server. Please wait...");
                Thread.Sleep(1000);
                Console.WriteLine($"Shut down is complete on {DateTimeOffset.UtcNow}.");
                Environment.Exit(-1);
            }
        }

        private static void InitSerilog()
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;

            Console.Title = OrgConstants.FullName;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Error)
                .MinimumLevel.Override("System", LogEventLevel.Error)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Error)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:w4}] {Message:lj}{NewLine}{Exception}", theme: AnsiConsoleTheme.Code)
                .CreateLogger();
        }

        private static Task<string?> ReadAnswerAsync(string question, params string[]? answers)
        {
            // Print the question
            if (answers != null)
            {
                Console.Write($"{question} ({string.Join('/', answers)}): ");
            }
            else
            {
                Console.Write($"{question}: ");
            }

            // Check answer
            string? answer = Console.ReadLine();
            if (answers != null)
            {
                // Special command
                if (answer == "exit")
                {
                    return Task.FromResult<string?>(null);
                }

                if (answers.Contains(answer))
                {
                    return Task.FromResult(answer);
                }
                else
                {
                    Log.Error("The answer is not valid. answers={0}", answers.JoinSpace());
                    return ReadAnswerAsync(question, answers);
                }
            }
            else
            {
                return Task.FromResult(answer);
            }
        }

        private static void WriteHeaders()
        {
            Console.WriteLine("---------------------------------------------------------------------------------------------");
            Console.WriteLine("Copyright (c). All Rights Reserved. Perpetual Intelligence L.L.C.");
            Console.WriteLine("https://perpetualintelligence.com");
            Console.WriteLine("https://api.perpetualintelligence.com");
            Console.WriteLine("https://oneimlx.com");
            Console.WriteLine("---------------------------------------------------------------------------------------------");

            Log.Information("Starting {0} server...", "pi.cli();");
            Log.Information("Version={0}", typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "<none>");

            Thread.Sleep(500);
        }
    }
}
