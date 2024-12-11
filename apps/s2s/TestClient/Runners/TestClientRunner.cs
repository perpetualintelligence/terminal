using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Apps.TestClient.Runners
{
    [CommandDescriptor("tc", "Test client", "Sample test client for testing the server.", Commands.CommandType.Root, Commands.CommandFlags.None)]
    public class TestClientRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        public TestClientRunner(ITerminalConsole terminalConsole, ILogger<TestClientRunner> logger)
        {
            this.terminalConsole = terminalConsole;
            this.logger = logger;
        }

        public override Task<CommandRunnerResult> RunCommandAsync(CommandRouterContext context)
        {
            terminalConsole.WriteLineAsync("Test client");
            return Task.FromResult(new CommandRunnerResult());
        }

        private readonly ILogger<TestClientRunner> logger;
        private readonly ITerminalConsole terminalConsole;
    }
}
