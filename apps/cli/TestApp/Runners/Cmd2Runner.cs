﻿using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands;
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
    [CommandDescriptor("cmd2", "Command 2", "Command2 description.", CommandType.SubCommand, CommandFlags.None)]
    public class Cmd2Runner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        private readonly ITerminalConsole terminalConsole;
        private readonly ILogger<Cmd2Runner> logger;

        public Cmd2Runner(ITerminalConsole terminalConsole, ILogger<Cmd2Runner> logger)
        {
            this.terminalConsole = terminalConsole;
            this.logger = logger;
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            await terminalConsole.WriteLineAsync("Command2 of Group2 called.");
            return new CommandRunnerResult();
        }
    }
}