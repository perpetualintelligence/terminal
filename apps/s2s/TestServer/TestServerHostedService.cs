using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Hosting;
using OneImlx.Terminal.Runtime;
using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.TestServer
{
    /// <summary>
    /// The <see cref="TerminalHostedService"/> for the test app.
    /// </summary>
    public sealed class TestServerHostedService : TerminalHostedService
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="serviceProvider">The DI service provider.</param>
        /// <param name="terminalOptions">The terminal configuration options.</param>
        /// <param name="terminalConsole">The terminal console.</param>
        /// <param name="logger">The logger.</param>
        public TestServerHostedService(
            IHostApplicationLifetime hostApplicationLifetime,
            IServiceProvider serviceProvider,
            IOptions<TerminalOptions> terminalOptions,
            ITerminalConsole terminalConsole,
            IConfiguration configuration,
            ITerminalExceptionHandler exceptionHandler,
            ILogger<TerminalHostedService> logger) : base(serviceProvider, terminalOptions, terminalConsole, exceptionHandler, logger)
        {
            this.hostApplicationLifetime = hostApplicationLifetime;
            this.configuration = configuration;
        }

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
            // Set title
            Console.Title = "Test Server";

            // These are async calls, but we are blocking here for as the of the test.
            string mode = configuration["testserver:mode"] ?? "unknown";
            TerminalConsole.WriteLineAsync("Test server started on {0}.", DateTime.UtcNow.ToLocalTime().ToString()).Wait();
            TerminalConsole.WriteLineColorAsync(ConsoleColor.Blue, "Communication Protocol={0}", mode).Wait();
            TerminalConsole.WriteLineColorAsync(ConsoleColor.Blue, "Response Handling Disabled={0}", Options.Value.Router.DisableResponse.ToString()).Wait();
        }

        /// <summary>
        /// The <see cref="IHostApplicationLifetime.ApplicationStopped"/> handler.
        /// </summary>
        private void OnStopped()
        {
            TerminalConsole.WriteLineColorAsync(ConsoleColor.Red, "Test server stopped on {0}.", DateTime.UtcNow.ToLocalTime().ToString()).Wait();
        }

        /// <summary>
        /// The <see cref="IHostApplicationLifetime.ApplicationStopping"/> handler.
        /// </summary>
        private void OnStopping()
        {
            TerminalConsole.WriteLineAsync("Stopping server...").Wait();
        }

        private readonly IConfiguration configuration;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
    }
}
