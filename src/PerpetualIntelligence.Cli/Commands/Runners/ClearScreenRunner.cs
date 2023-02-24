/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Runtime;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Runners
{
    /// <summary>
    /// The clear screen command runner.
    /// </summary>
    public class ClearScreenRunner : CommandRunner<CommandRunnerResult>
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="terminalLogger">The terminal logger.</param>
        /// <param name="logger">The logger.</param>
        public ClearScreenRunner(CliOptions options, ITerminalLogger terminalLogger, ILogger<ExitRunner> logger)
        {
        }

        /// <inheritdoc/>
        public override Task<CommandRunnerResult> RunAsync(CommandRunnerContext context)
        {
            Console.Clear();
            return Task.FromResult(new CommandRunnerResult());
        }
    }
}