using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.TestAuth.Runners
{
    /// <summary>
    /// Command runner for the <c>test auth</c> group in the test app.
    /// </summary>
    [CommandOwners("test")]
    [CommandDescriptor("auth", "Auth group", "Test auth group description.", CommandType.GroupCommand, CommandFlags.None)]
    public class AuthRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        private readonly ITerminalConsole _terminalConsole;
        private readonly ILogger<AuthRunner> _logger;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="terminalConsole">Terminal console service.</param>
        /// <param name="logger">Logger instance for logging.</param>
        public AuthRunner(ITerminalConsole terminalConsole, ILogger<AuthRunner> logger)
        {
            _terminalConsole = terminalConsole;
            _logger = logger;
        }

        /// <summary>
        /// Runs the command for the 'test auth' group.
        /// </summary>
        /// <param name="context">Command runner context.</param>
        /// <returns>Command runner result.</returns>
        public override async Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            await _terminalConsole.WriteLineAsync("Auth group command called.");

            return new CommandRunnerResult();
        }
    }
}
