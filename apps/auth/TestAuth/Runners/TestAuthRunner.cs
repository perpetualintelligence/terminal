using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.TestAuth.Runners
{
    /// <summary>
    /// The root <c>test</c> runner for the TestApp.
    /// </summary>
    [CommandDescriptor("test", "Test App", "Test application description.", Commands.CommandType.Root, Commands.CommandFlags.None)]
    [OptionDescriptor("version", nameof(String), "Test version description", Commands.OptionFlags.None, "v")]
    [CommandChecker(typeof(CommandChecker))]
    public class TestRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="terminalConsole">Terminal console service.</param>
        /// <param name="logger">Logger instance for logging.</param>
        public TestRunner(ITerminalConsole terminalConsole, ILogger<TestRunner> logger)
        {
            _terminalConsole = terminalConsole;
            _logger = logger;
        }

        /// <summary>
        /// Runs the root command for the TestApp.
        /// </summary>
        /// <param name="context">Command runner context.</param>
        /// <returns>Command runner result.</returns>
        public override async Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            await _terminalConsole.WriteLineAsync("Test root command called.");

            // Get the version option value
            if (context.Command.TryGetOptionValue("version", out string? version))
            {
                await _terminalConsole.WriteLineAsync("Version option passed.");
            }

            return new CommandRunnerResult();
        }

        private readonly ILogger<TestRunner> _logger;
        private readonly ITerminalConsole _terminalConsole;
    }
}
