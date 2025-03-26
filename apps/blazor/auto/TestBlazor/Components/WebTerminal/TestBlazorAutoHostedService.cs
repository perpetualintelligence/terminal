using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Apps.TestBlazor.Components.WebTerminal
{
    /// <summary>
    /// The <see cref="TerminalHostedService"/> for the test app.
    /// </summary>
    public sealed class TestBlazorAutoHostedService : TerminalHostedService
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="serviceProvider">The DI service provider.</param>
        /// <param name="options">The terminal configuration options.</param>
        /// <param name="terminalConsole">The terminal console.</param>
        /// <param name="logger">The logger.</param>
        public TestBlazorAutoHostedService(
            IServiceProvider serviceProvider,
            IHostApplicationLifetime hostApplicationLifetime,
            IOptions<TerminalOptions> options,
            ITerminalConsole terminalConsole,
            ITerminalExceptionHandler exceptionHandler,
            ILogger<TerminalHostedService> logger) : base(serviceProvider, options, terminalConsole, exceptionHandler, logger)
        {
            this.hostApplicationLifetime = hostApplicationLifetime;
        }

        /// <inheritdoc/>
        protected override Task ConfigureLifetimeAsync()
        {
            hostApplicationLifetime.ApplicationStarted.Register(OnStarted);
            hostApplicationLifetime.ApplicationStopping.Register(OnStopping);
            hostApplicationLifetime.ApplicationStopped.Register(OnStopped);
            return Task.CompletedTask;
        }

        /// <summary>
        /// The <see cref="IHostApplicationLifetime.ApplicationStarted"/> handler.
        /// </summary>
        private void OnStarted()
        {
            // These are async calls, but we are blocking here for as the of the test.
            TerminalConsole.WriteLineAsync("Blazor hosted web assembly terminal started on {0}.", DateTime.UtcNow.ToLocalTime().ToString()).Wait();
        }

        /// <summary>
        /// The <see cref="IHostApplicationLifetime.ApplicationStopped"/> handler.
        /// </summary>
        private void OnStopped()
        {
            TerminalConsole.WriteLineColorAsync(ConsoleColor.Red, "Blazor hosted web assembly terminal stopped on {0}.", DateTime.UtcNow.ToLocalTime().ToString()).Wait();
        }

        /// <summary>
        /// The <see cref="IHostApplicationLifetime.ApplicationStopping"/> handler.
        /// </summary>
        private void OnStopping()
        {
            TerminalConsole.WriteLineAsync("Stopping web terminal...").Wait();
        }

        private readonly IHostApplicationLifetime hostApplicationLifetime;
    }
}
