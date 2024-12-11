using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.TestClient.Runners
{
    [CommandOwners("tc")]
    [CommandDescriptor("send", "Send", "Send commands to the server.", Commands.CommandType.Group, Commands.CommandFlags.None)]
    public class SendRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        public SendRunner(ITerminalConsole terminalConsole)
        {
            this.terminalConsole = terminalConsole;
        }

        public override Task<CommandRunnerResult> RunCommandAsync(CommandRouterContext context)
        {
            terminalConsole.WriteLineAsync("Sends messages to the test server.");
            return Task.FromResult(new CommandRunnerResult());
        }

        private readonly ITerminalConsole terminalConsole;
    }
}
