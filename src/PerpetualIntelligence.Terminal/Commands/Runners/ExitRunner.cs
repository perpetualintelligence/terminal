/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/
/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Hosting;
using PerpetualIntelligence.Terminal.Runtime;
using System.Threading;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Runners
{
    /// <summary>
    /// The exit command runner.
    /// </summary>
    public class ExitRunner : CommandRunner<CommandRunnerResult>
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="terminalConsole">The terminal console.</param>
        public ExitRunner(IHost host, ITerminalConsole terminalConsole)
        {
            this.host = host;
            this.terminalConsole = terminalConsole;
        }

        /// <inheritdoc/>
        public override async Task<CommandRunnerResult> RunAsync(CommandRunnerContext context)
        {
            // Confirm
            string? answer = await terminalConsole.ReadAnswerAsync("Are you sure", "y", "n");
            if (answer == "y" || answer == "Y")
            {
                await host.StopAsync(CancellationToken.None);
            }

            return new CommandRunnerResult();
        }

        private readonly IHost host;
        private readonly ITerminalConsole terminalConsole;
    }
}