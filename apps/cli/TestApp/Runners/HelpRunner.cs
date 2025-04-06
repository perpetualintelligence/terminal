using System.Threading.Tasks;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using OneImlx.Terminal.Stores;

namespace OneImlx.Terminal.Apps.Test.Runners
{
    /// <summary>
    /// Runs native OS commands.
    /// </summary>
    [CommandDescriptor("help", "Help Command", "Displays all supported commands.", CommandType.NativeCommand, CommandFlags.None)]
    public class HelpRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HelpRunner"/> class.
        /// </summary>
        public HelpRunner(ITerminalConsole console, ITerminalCommandStore commandStore)
        {
            _console = console;
            this.commandStore = commandStore;
        }

        /// <inheritdoc/>
        public override async Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            var commands = await commandStore.AllAsync();

            foreach (var command in commands)
            {
                await _console.WriteLineAsync($"{command.Key} ({command.Value.Name}) --> {command.Value.Description}");
            }
            return CommandRunnerResult.Empty();
        }

        private readonly ITerminalConsole _console;
        private readonly ITerminalCommandStore commandStore;
    }
}
