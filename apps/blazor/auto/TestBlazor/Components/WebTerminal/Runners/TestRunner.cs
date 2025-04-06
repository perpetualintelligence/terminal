using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.TestBlazor.Components.WebTerminal.Runners
{
    /// <summary>
    /// The root <c>test</c> runner for the TestApp.
    /// </summary>
    [CommandDescriptor("test", "Test App", "Test application description.", CommandType.RootCommand, CommandFlags.None)]
    [OptionDescriptor("version", nameof(String), "Test version description", OptionFlags.None, "v")]
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

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            await terminalConsole.WriteLineAsync("Test root command called.");

            // Get the version option value
            if (context.EnsureCommand().TryGetOptionValue("version", out string? version))
            {
                await terminalConsole.WriteLineAsync("Version option passed.");
            }

            return new CommandRunnerResult();
        }
    }
}