using System;
using System.Diagnostics;
using System.Threading.Tasks;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Apps.Test.Runners
{
    /// <summary>
    /// Runs native OS commands.
    /// </summary>
    [CommandDescriptor("run", "Run Command", "Runs a native OS command.", CommandType.NativeCommand, CommandFlags.None)]
    [ArgumentDescriptor(0, "cmd", nameof(String), "The full native command to execute, e.g., 'ls -all'", ArgumentFlags.Required)]
    public class RunRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RunRunner"/> class.
        /// </summary>
        public RunRunner(ITerminalConsole terminalConsole)
        {
            this.terminalConsole = terminalConsole;
        }

        /// <inheritdoc/>
        public override async Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            var command = context.EnsureParsedCommand().Command;
            var osCommand = command.GetRequiredArgumentValue<string>("cmd");

            await terminalConsole.WriteLineColorAsync(ConsoleColor.Magenta, $"Running OS command: {osCommand}");

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"{osCommand}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };

            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(error))
            {
                throw new InvalidOperationException($"Command error: {error.Trim()}");
            }

            await terminalConsole.WriteLineColorAsync(ConsoleColor.Green, $"OS command complete: {output}");
            return new CommandRunnerResult(output);
        }

        private readonly ITerminalConsole terminalConsole;
    }
}
