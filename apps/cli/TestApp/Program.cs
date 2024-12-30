using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneImlx.Shared.Licensing;
using OneImlx.Terminal.Apps.Test.Runners;
using OneImlx.Terminal.Extensions;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Stores;
using Serilog;

namespace OneImlx.Terminal.Apps.Test
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
            collection.AddHostedService<TestAppHostedService>();

            // We are using on-line license so configure HTTP
            collection.AddHttpClient("demo-http");

            // NOTE: Specify your demo or commercial license file. Specify your application id.
            TerminalTextHandler textHandler = new(StringComparison.OrdinalIgnoreCase, Encoding.Unicode);
            ITerminalBuilder terminalBuilder = collection.AddTerminalConsole<TerminalInMemoryCommandStore, TerminalConsoleHelpProvider, TerminalConsoleExceptionHandler, TerminalSystemConsole>(textHandler,
                options =>
                {
                    options.Id = TerminalIdentifiers.TestApplicationId;

                    options.Driver.Enabled = true;
                    options.Driver.RootId = "test";

                    options.Licensing.LicenseFile = "C:\\this\\lic\\oneimlx-terminal-demo-test.json";
                    options.Licensing.LicensePlan = TerminalLicensePlans.Demo;
                    options.Licensing.Deployment = null;
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

        private static bool IsNewTerminal()
        {
            try
            {
                int parentPid = -1;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows: Use WMI to fetch ParentProcessId
                    using var searcher = new System.Management.ManagementObjectSearcher(
                        $"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {Environment.ProcessId}");
                    foreach (var obj in searcher.Get())
                    {
                        parentPid = Convert.ToInt32(obj["ParentProcessId"]);
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                         RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // Linux / macOS: Use 'ps' command to get parent PID
                    using var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "sh",
                            Arguments = $"-c \"ps -o ppid= -p {Process.GetCurrentProcess().Id}\"",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    if (int.TryParse(output.Trim(), out int parsedPid))
                        parentPid = parsedPid;
                }

                if (parentPid > 0)
                {
                    using var parentProcess = Process.GetProcessById(parentPid);
                    string parentName = parentProcess.ProcessName.ToLowerInvariant();

                    // Handle specific known console/debug parents
                    return parentName switch
                    {
                        "vsdebugconsole" => true,       // Visual Studio Debug Console
                        "explorer" => true,             // Windows File Explorer
                        "finder" => true,               // macOS Finder
                        "nautilus" => true,             // Linux Nautilus (File Explorer)
                        _ => false                      // Default to terminal
                    };
                }

                return false; // Fallback if no valid parent PID was found
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error determining terminal state: {ex.Message}");
                return false; // Default to terminal if any error occurs
            }
        }

        private static void Main(string[] args)
        {
            bool newTerminal = IsNewTerminal();

            // Allows cancellation for the entire terminal and individual commands.
            CancellationTokenSource terminalTokenSource = new();
            CancellationTokenSource commandTokenSource = new();

            // Setup the terminal context and run the router indefinitely as a console.
            // NOTE: Driver is enabled, so you can run the terminal as a native driver program. Ensure args are passed.
            Dictionary<string, object> customProperties = new()
            {
                { "new_terminal", newTerminal }
            };
            TerminalConsoleRouterContext consoleRouterContext = new(TerminalStartMode.Console, terminalTokenSource.Token, commandTokenSource.Token, routeOnce: !newTerminal, customProperties, args);

            // Start the host builder and run terminal router till canceled.
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(ConfigureAppConfigurationDelegate)
                .ConfigureLogging(ConfigureLoggingDelegate)
                .ConfigureServices(ConfigureServicesDelegate)
                .RunTerminalRouter<TerminalConsoleRouter, TerminalConsoleRouterContext>(consoleRouterContext);
        }

        private static void RegisterCommands(ITerminalBuilder terminalBuilder)
        {
            // Root Command
            terminalBuilder.DefineCommand<TestRunner>("test", "Test command", "Test Description", Commands.CommandType.RootCommand, Commands.CommandFlags.None)
                           .DefineArgument(1, "arg1", nameof(String), "The first argument", Commands.ArgumentFlags.None)
                               .Add()
                               .DefineArgument(2, "arg2", nameof(Int32), "The second argument", Commands.ArgumentFlags.None)
                                   .Add()
                               .DefineOption("version", nameof(String), "The version option", Commands.OptionFlags.None, "v")
                                   .Add()
                           .Add();

            // Grp1 Command
            terminalBuilder.DefineCommand<Grp1Runner>("grp1", "Test Group1", "Test Group1 Description", Commands.CommandType.GroupCommand, Commands.CommandFlags.None)
                           .Owners(new Commands.OwnerIdCollection("test"))
                           .Add();

            // Cmd1 Command
            terminalBuilder.DefineCommand<Cmd1Runner>("cmd1", "Test Command1", "Test Command2 Description", Commands.CommandType.SubCommand, Commands.CommandFlags.None)
                           .Owners(new Commands.OwnerIdCollection("grp1"))
                           .Add();

            // Grp2 Command
            terminalBuilder.DefineCommand<Grp2Runner>("grp2", "Test Group2", "Test Group1 Description", Commands.CommandType.GroupCommand, Commands.CommandFlags.None)
                           .Owners(new Commands.OwnerIdCollection("grp1"))
                           .Add();

            // Cmd2 Command
            terminalBuilder.DefineCommand<Cmd2Runner>("cmd2", "Test Command2", "Test Command2 Description", Commands.CommandType.SubCommand, Commands.CommandFlags.None)
                           .Owners(new Commands.OwnerIdCollection("grp2"))
                           .Add();

            // Help Command
            terminalBuilder.DefineCommand<HelpRunner>("help", "Help Command", "Displays all supported commands.", Commands.CommandType.NativeCommand, Commands.CommandFlags.None)
                           .Add();

            // Exit Command
            terminalBuilder.DefineCommand<ExitRunner>("exit", "Exit Command", "Exits the terminal.", Commands.CommandType.NativeCommand, Commands.CommandFlags.None)
                           .Add();

            // Clear Command
            terminalBuilder.DefineCommand<ClearRunner>("clear", "Clear Command", "Clears the terminal.", Commands.CommandType.NativeCommand, Commands.CommandFlags.None)
                           .Add();

            // Run Command
            terminalBuilder.DefineCommand<RunRunner>("run", "Run Command", "Runs a native OS command.", Commands.CommandType.NativeCommand, Commands.CommandFlags.None)
                           .DefineArgument(1, "cmd", nameof(String), "The command to run", Commands.ArgumentFlags.None)
                               .Add()
                           .Add();
        }
    }
}
