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

using PerpetualIntelligence.Terminal.Runtime;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Runners
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
        public override async Task<CommandRunnerResult> RunAsync(CommandRunnerContext context)
        {
            await terminalConsole.ClearAsync();
            return new CommandRunnerResult();
        }
    }
}