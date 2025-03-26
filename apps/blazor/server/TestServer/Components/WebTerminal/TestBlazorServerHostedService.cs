using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Apps.TestServer.Components.WebTerminal
{
    /// <summary>
    /// The <see cref="TerminalHostedService"/> for the test app.
    /// </summary>
    public sealed class TestBlazorServerHostedService : TerminalHostedService
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="serviceProvider">The DI service provider.</param>
        /// <param name="options">The terminal configuration options.</param>
        /// <param name="terminalConsole">The terminal console.</param>
        /// <param name="logger">The logger.</param>
        public TestBlazorServerHostedService(
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

        private void OnStarted()
        {
            // These are async calls, but we are blocking here for as the of the test.
            TerminalConsole.WriteLineAsync("Blazor server web terminal started on {0}.", DateTime.UtcNow.ToLocalTime().ToString()).Wait();
        }

        private void OnStopped()
        {
            TerminalConsole.WriteLineColorAsync(ConsoleColor.Red, "Blazor server web terminal stopped on {0}.", DateTime.UtcNow.ToLocalTime().ToString()).Wait();
        }

        private void OnStopping()
        {
            TerminalConsole.WriteLineAsync("Stopping web terminal...").Wait();
        }

        private readonly IHostApplicationLifetime hostApplicationLifetime;
    }
}
