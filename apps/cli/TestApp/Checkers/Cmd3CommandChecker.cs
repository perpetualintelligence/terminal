using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.TestApp.Checkers
{
    public class Cmd3CommandChecker : ICommandChecker
    {
        public Cmd3CommandChecker(ITerminalConsole terminalConsole)
        {
            this.terminalConsole = terminalConsole;
        }

        public Task<CommandCheckerResult> CheckCommandAsync(CommandCheckerContext context)
        {
            terminalConsole.WriteLineAsync("Cmd3 custom checker called.");
            return Task.FromResult(new CommandCheckerResult());
        }

        private readonly ITerminalConsole terminalConsole;
    }
}
