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
    /// The root <c>test</c> runner for the TestApp.
    /// </summary>
    [CommandDescriptor("test", "Test App", "Test application description.", Commands.CommandType.Root, Commands.CommandFlags.None)]
    [OptionDescriptor("version", nameof(String), "Test version description", Commands.OptionFlags.None, "v")]
    [CommandChecker(typeof(CommandChecker))]
    public class TestRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        private readonly ITerminalConsole terminalConsole;
        private readonly ILogger<TestRunner> logger;

        public TestRunner(ITerminalConsole terminalConsole, ILogger<TestRunner> logger)
        {
            this.terminalConsole = terminalConsole;
            this.logger = logger;
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            await terminalConsole.WriteLineAsync("Test root command called.");

            // Get the version option value
            if (context.Command.TryGetOptionValue("version", out string? version))
            {
                await terminalConsole.WriteLineAsync("Version option passed.");
            }

            return new CommandRunnerResult();
        }
    }
}