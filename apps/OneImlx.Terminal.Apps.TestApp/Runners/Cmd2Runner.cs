/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Apps.TestApp.Runners
{
    /// <summary>
    /// The sub-command <c>cmd2</c> runner for the TestApp.
    /// </summary>
    [CommandOwners("grp2")]
    [CommandDescriptor("cmd2", "Command 2", "Command2 description.", Commands.CommandType.SubCommand, Commands.CommandFlags.None)]
    [CommandChecker(typeof(CommandChecker))]
    public class Cmd2Runner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        private readonly ITerminalConsole terminalConsole;
        private readonly ILogger<Cmd2Runner> logger;

        public Cmd2Runner(ITerminalConsole terminalConsole, ILogger<Cmd2Runner> logger)
        {
            this.terminalConsole = terminalConsole;
            this.logger = logger;
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            await terminalConsole.WriteLineAsync("Command2 of Group2 called.");
            return new CommandRunnerResult();
        }
    }
}