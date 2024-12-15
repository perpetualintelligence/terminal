using Microsoft.Extensions.Hosting;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Declarative;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Apps.TestClient.Runners
{
    /// <summary>
    /// Runs native OS commands.
    /// </summary>
    [CommandDescriptor("exit", "Exit", "Exits the client terminal application.", Commands.CommandType.NativeCommand, Commands.CommandFlags.None)]
    public class ExitRunner : CommandRunner<CommandRunnerResult>, IDeclarativeRunner
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExitRunner"/> class.
        /// </summary>
        public ExitRunner(ITerminalConsole terminalConsole, IHostApplicationLifetime applicationLifetime)
        {
            this.terminalConsole = terminalConsole;
            _applicationLifetime = applicationLifetime;
        }

        /// <inheritdoc/>
        public override async Task<CommandRunnerResult> RunCommandAsync(CommandContext context)
        {
            string answer = await terminalConsole.ReadAnswerAsync("Are you sure you want to exit ?", "y", "Y");
            if (answer == "y" || answer == "Y")
            {
                _applicationLifetime.StopApplication();
                await Task.Delay(2000);
            }

            return CommandRunnerResult.Empty();
        }

        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly ITerminalConsole terminalConsole;
    }
}
