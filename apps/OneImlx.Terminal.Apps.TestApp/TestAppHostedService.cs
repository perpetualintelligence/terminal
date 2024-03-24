/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Apps.TestApp
{
    /// <summary>
    /// The <see cref="TerminalHostedService"/> for the test app.
    /// </summary>
    public sealed class TestAppHostedService : TerminalHostedService
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="serviceProvider">The DI service provider.</param>
        /// <param name="options">The terminal configuration options.</param>
        /// <param name="terminalConsole">The terminal console.</param>
        /// <param name="logger">The logger.</param>
        public TestAppHostedService(
            IServiceProvider serviceProvider,
            TerminalOptions options,
            ITerminalConsole terminalConsole,
            ILogger<TerminalHostedService> logger) : base(serviceProvider, options, terminalConsole, logger)
        {
        }

        /// <summary>
        /// Perform custom configuration option checks at startup.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        protected override Task CheckHostApplicationConfigurationAsync(TerminalOptions options)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// The <see cref="IHostApplicationLifetime.ApplicationStarted"/> handler.
        /// </summary>
        protected override void OnStarted()
        {
            // These are async calls, but we are blocking here for as the  of the test.
            TerminalConsole.WriteLineAsync("Application started on {0}.", DateTime.UtcNow.ToLocalTime().ToString()).Wait();
            TerminalConsole.WriteLineAsync().Wait();
        }

        /// <summary>
        /// The <see cref="IHostApplicationLifetime.ApplicationStopped"/> handler.
        /// </summary>
        protected override void OnStopped()
        {
            TerminalConsole.WriteLineColorAsync(ConsoleColor.Red, "Application stopped on {0}.", DateTime.UtcNow.ToLocalTime().ToString()).Wait();
        }

        /// <summary>
        /// The <see cref="IHostApplicationLifetime.ApplicationStopping"/> handler.
        /// </summary>
        protected override void OnStopping()
        {
            TerminalConsole.WriteLineAsync("Stopping application...").Wait();
        }

        /// <summary>
        /// Print <c>cli</c> terminal header.
        /// </summary>
        /// <returns></returns>
        protected override async Task PrintHostApplicationHeaderAsync()
        {
            await TerminalConsole.WriteLineAsync("---------------------------------------------------------------------------------------------");
            await TerminalConsole.WriteLineAsync("Copyright (c) Test App. All Rights Reserved.");
            await TerminalConsole.WriteLineAsync("For license, terms, and data policies, go to:");
            await TerminalConsole.WriteLineAsync("https://mytestapp.com");
            await TerminalConsole.WriteLineAsync("---------------------------------------------------------------------------------------------");

            await TerminalConsole.WriteLineAsync($"Starting application...");
        }

        /// <summary>
        /// Print host application licensing information.
        /// </summary>
        /// <param name="license">The extracted license.</param>
        /// <returns></returns>
        protected override Task PrintHostApplicationLicensingAsync(License license)
        {
            // Print custom licensing info or remove it completely.
            return base.PrintHostApplicationLicensingAsync(license);
        }
    }
}