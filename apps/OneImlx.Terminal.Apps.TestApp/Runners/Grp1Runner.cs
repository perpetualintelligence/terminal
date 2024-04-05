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
    /// The group <c>grp1</c> runner for the TestApp.
    /// </summary>
    [CommandOwners("test")]
    [CommandDescriptor("grp1", "Group 1", "Group1 description.", Commands.CommandType.Group, Commands.CommandFlags.None)]
    [CommandRunner(typeof(Grp1Runner))]
    [CommandChecker(typeof(CommandChecker))]
    public class Grp1Runner : CommandRunner<CommandRunnerResult>, IDeclarativeTarget
    {
        private readonly ITerminalConsole terminalConsole;
        private readonly ILogger<Grp1Runner> logger;

        public Grp1Runner(ITerminalConsole terminalConsole, ILogger<Grp1Runner> logger)
        {
            this.terminalConsole = terminalConsole;
            this.logger = logger;
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            await terminalConsole.WriteLineAsync("Group1 command called.");
            return new CommandRunnerResult();
        }
    }
}