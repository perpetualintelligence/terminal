using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Apps.Runners
{
    [CommandDescriptor("tc", "Test client", "Sample test client for testing the server.", Commands.CommandType.Root, Commands.CommandFlags.None)]
    public class TestClientRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        public TestClientRunner(ITerminalConsole terminalConsole, ILogger<TestClientRunner> logger)
        {
            this.terminalConsole = terminalConsole;
            this.logger = logger;
        }

        public override Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            terminalConsole.WriteLineAsync("Test client");
            return Task.FromResult(CommandRunnerResult.NoProcessing);
        }

        private readonly ILogger<TestClientRunner> logger;
        private readonly ITerminalConsole terminalConsole;
    }
}
