using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Apps.TestServer.Runners
{
    /// <summary>
    /// The root <c>test</c> runner for the TestServer.
    /// </summary>
    [CommandDescriptor("ts", "Test Server", "Test server description.", Commands.CommandType.Root, Commands.CommandFlags.None)]
    [OptionDescriptor("version", nameof(String), "Test server version description", Commands.OptionFlags.None, "v")]
    public class TestServerRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        private readonly ITerminalConsole terminalConsole;
        private readonly ILogger<TestServerRunner> logger;

        public TestServerRunner(ITerminalConsole terminalConsole, ILogger<TestServerRunner> logger)
        {
            this.terminalConsole = terminalConsole;
            this.logger = logger;
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            await terminalConsole.WriteLineAsync("Test server root command called.");

            // Get the version option value
            if (context.Command.TryGetOptionValue("version", out string? version))
            {
                await terminalConsole.WriteLineAsync("Version option passed.");
            }

            return new CommandRunnerResult();
        }
    }
}