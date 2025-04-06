﻿using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.TestServer.Runners
{
    /// <summary>
    /// The root <c>test</c> runner for the TestServer.
    /// </summary>
    [CommandDescriptor("ts", "Test Server", "Test server description.", CommandType.RootCommand, CommandFlags.None)]
    [OptionDescriptor("version", nameof(String), "Test server version description", OptionFlags.None, "v")]
    public class TestServerRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        private readonly ITerminalConsole terminalConsole;
        private readonly ILogger<TestServerRunner> logger;

        public TestServerRunner(ITerminalConsole terminalConsole, ILogger<TestServerRunner> logger)
        {
            this.terminalConsole = terminalConsole;
            this.logger = logger;
        }

        public override async Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            await terminalConsole.WriteLineAsync("Test server root command called.");

            // Get the version option value
            if (context.EnsureParsedCommand().Command.TryGetOptionValue("version", out string? version))
            {
                await terminalConsole.WriteLineAsync("Version option passed.");
            }

            return new CommandRunnerResult();
        }
    }
}