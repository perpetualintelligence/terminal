using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.Test.Runners
{
    /// <summary>
    /// The group <c>grp1</c> runner for the TestApp.
    /// </summary>
    [CommandOwners("grp1")]
    [CommandDescriptor("grp2", "Group 2", "Group2 description.", Commands.CommandType.GroupCommand, Commands.CommandFlags.None)]
    [CommandChecker(typeof(CommandChecker))]
    public class Grp2Runner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        private readonly ITerminalConsole terminalConsole;
        private readonly ILogger<Grp2Runner> logger;

        public Grp2Runner(ITerminalConsole terminalConsole, ILogger<Grp2Runner> logger)
        {
            this.terminalConsole = terminalConsole;
            this.logger = logger;
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            await terminalConsole.WriteLineAsync("Group2 command called.");
            return new CommandRunnerResult();
        }
    }
}