using System.Threading.Tasks;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Apps.TestClient.Runners
{
    /// <summary>
    /// Clears the console.
    /// </summary>
    [CommandDescriptor("cls", "Clear Console", "Clears the console.", CommandType.NativeCommand, CommandFlags.None)]
    public class ClsRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClsRunner"/> class.
        /// </summary>
        /// <param name="terminalConsole">The terminal console.</param>
        public ClsRunner(ITerminalConsole terminalConsole)
        {
            this.terminalConsole = terminalConsole;
        }

        /// <inheritdoc/>
        public override async Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            await terminalConsole.ClearAsync();
            return await CommandRunnerResult.EmptyAsync();
        }

        private readonly ITerminalConsole terminalConsole;
    }
}
