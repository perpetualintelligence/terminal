/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Runners
{
    /// <summary>
    /// The clear screen command runner.
    /// </summary>
    public class ClearScreenRunner : CommandRunner<CommandRunnerResult>
    {
        private readonly ITerminalConsole terminalConsole;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ClearScreenRunner(ITerminalConsole terminalConsole)
        {
            this.terminalConsole = terminalConsole;
        }

        /// <inheritdoc/>
        public override async Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            await terminalConsole.ClearAsync();
            return new CommandRunnerResult();
        }
    }
}