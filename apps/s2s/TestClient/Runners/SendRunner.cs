using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.Runners
{
    [CommandOwners("tc")]
    [CommandDescriptor("send", "Send", "Send commands to the server.", Commands.CommandType.Group, Commands.CommandFlags.None)]
    public class SendRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        public SendRunner(ITerminalConsole terminalConsole)
        {
            this.terminalConsole = terminalConsole;
        }

        public override Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            terminalConsole.WriteLineAsync("Sends messages to the test server.");
            return Task.FromResult(CommandRunnerResult.NoProcessing);
        }

        private readonly ITerminalConsole terminalConsole;
    }
}
