/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Runners
{
    /// <summary>
    /// The <c>run</c> command runner.
    /// </summary>
    public class RunRunner : CommandRunner
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public RunRunner(CliOptions options, ILogger logger) : base(options, logger)
        {
        }

        /// <inheritdoc/>
        public override Task<CommandRunnerResult> RunAsync(CommandRunnerContext context)
        {
            // TODO;
            return Task.FromResult(new CommandRunnerResult());
        }
    }
}
