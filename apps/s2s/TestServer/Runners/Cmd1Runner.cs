using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Apps.TestServer.Runners
{
    /// <summary>
    /// The sub-command <c>cmd1</c> runner for the TestServer.
    /// </summary>
    [CommandOwners("grp1")]
    [CommandDescriptor("cmd1", "Command 1", "Command1 description.", Commands.CommandType.SubCommand, Commands.CommandFlags.None)]
    [CommandChecker(typeof(CommandChecker))]
    public class Cmd1Runner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        public Cmd1Runner(ITerminalConsole terminalConsole, ILogger<Cmd1Runner> logger)
        {
            this.terminalConsole = terminalConsole;
            this.logger = logger;
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandRouterContext context)
        {
            await terminalConsole.WriteLineAsync("Command1 of Group1 called.");
            return new CommandRunnerResult("Response from cmd1");
        }

        private readonly ILogger<Cmd1Runner> logger;
        private readonly ITerminalConsole terminalConsole;
    }
}
