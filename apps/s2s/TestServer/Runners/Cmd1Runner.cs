﻿using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.TestServer.Runners
{
    /// <summary>
    /// The sub-command <c>cmd1</c> runner for the TestServer.
    /// </summary>
    [CommandOwners("grp1")]
    [CommandDescriptor("cmd1", "Command 1", "Command1 description.", Commands.CommandType.SubCommand, Commands.CommandFlags.None)]
    [CommandChecker(typeof(CommandChecker))]
    public class Cmd1Runner : CommandRunner<CommandRunnerResultSendToFile>, IDeclarativeRunner
    {
        private readonly ITerminalConsole terminalConsole;
        private readonly ILogger<Cmd1Runner> logger;

        public Cmd1Runner(ITerminalConsole terminalConsole, ILogger<Cmd1Runner> logger)
        {
            this.terminalConsole = terminalConsole;
            this.logger = logger;
        }

        public override async Task<CommandRunnerResultSendToFile> RunCommandAsync(CommandRunnerContext context)
        {
            await terminalConsole.WriteLineAsync("Command1 of Group1 called.");
            return new CommandRunnerResultSendToFile();
        }
    }
}