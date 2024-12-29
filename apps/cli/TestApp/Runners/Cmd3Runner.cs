using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Apps.Test.Checkers;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.Test.Runners
{
    /// <summary>
    /// The sub-command <c>cmd2</c> runner for the TestApp.
    /// </summary>
    [CommandOwners("grp2")]
    [CommandDescriptor("cmd3", "Command 3", "Command3 description.", Commands.CommandType.SubCommand, Commands.CommandFlags.None)]
    [CommandChecker(typeof(Cmd3CommandChecker))]
    public class Cmd3Runner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        private readonly ITerminalConsole terminalConsole;
        private readonly ILogger<Cmd2Runner> logger;

        public Cmd3Runner(ITerminalConsole terminalConsole, ILogger<Cmd2Runner> logger)
        {
            this.terminalConsole = terminalConsole;
            this.logger = logger;
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            await terminalConsole.WriteLineAsync("Command3 of Group2 called.");
            return new CommandRunnerResult();
        }
    }
}