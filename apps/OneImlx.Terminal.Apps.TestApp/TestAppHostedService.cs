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
        private readonly ITerminalConsole terminalConsole;

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
            ILogger<TerminalHostedService> logger) : base(serviceProvider, options, logger)
        {
            this.terminalConsole = terminalConsole;
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
            terminalConsole.WriteLineAsync("Application started on {0}.", DateTime.UtcNow.ToLocalTime().ToString()).Wait();
            terminalConsole.WriteLineAsync().Wait();
        }

        /// <summary>
        /// The <see cref="IHostApplicationLifetime.ApplicationStopped"/> handler.
        /// </summary>
        protected override void OnStopped()
        {
            terminalConsole.WriteLineColorAsync(ConsoleColor.Red, "Application stopped on {0}.", DateTime.UtcNow.ToLocalTime().ToString()).Wait();
        }

        /// <summary>
        /// The <see cref="IHostApplicationLifetime.ApplicationStopping"/> handler.
        /// </summary>
        protected override void OnStopping()
        {
            terminalConsole.WriteLineAsync("Stopping application...").Wait();
        }

        /// <summary>
        /// Print <c>cli</c> terminal header.
        /// </summary>
        /// <returns></returns>
        protected override async Task PrintHostApplicationHeaderAsync()
        {
            await terminalConsole.WriteLineAsync("---------------------------------------------------------------------------------------------");
            await terminalConsole.WriteLineAsync("Copyright (c) Test App. All Rights Reserved.");
            await terminalConsole.WriteLineAsync("For license, terms, and data policies, go to:");
            await terminalConsole.WriteLineAsync("https://mytestapp.com");
            await terminalConsole.WriteLineAsync("---------------------------------------------------------------------------------------------");

            await terminalConsole.WriteLineAsync($"Starting application...");
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