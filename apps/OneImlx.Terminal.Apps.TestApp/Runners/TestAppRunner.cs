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
    /// The root runner for the TestApp.
    /// </summary>
    [CommandDescriptor("test", "Test App", "Test application description.", Commands.CommandType.Root, Commands.CommandFlags.None)]
    [CommandRunner(typeof(TestAppRunner))]
    [CommandChecker(typeof(CommandChecker))]
    public class TestAppRunner : CommandRunner<CommandRunnerResult>, IDeclarativeTarget
    {
        private readonly ITerminalConsole terminalConsole;
        private readonly ILogger<TestAppRunner> logger;

        public TestAppRunner(ITerminalConsole terminalConsole, ILogger<TestAppRunner> logger)
        {
            this.terminalConsole = terminalConsole;
            this.logger = logger;
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            await terminalConsole.WriteLineAsync("Test root command called.");
            return new CommandRunnerResult();
        }
    }
}