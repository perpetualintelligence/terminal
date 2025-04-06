using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.TestServer.Components.WebTerminal.Runners
{
    /// <summary>
    /// CLears the current terminal buffer.
    /// </summary>
    [CommandDescriptor("clear", "Clear", "Clear description", CommandType.NativeCommand, CommandFlags.None)]
    public class ClearRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        public ClearRunner(ITerminalConsole terminalConsole)
        {
            this.terminalConsole = terminalConsole;
        }

        /// <summary>
        /// Clears the terminal buffer.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override async Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            await terminalConsole.ClearAsync();
            return new CommandRunnerResult();
        }

        private readonly ITerminalConsole terminalConsole;
    }
}
