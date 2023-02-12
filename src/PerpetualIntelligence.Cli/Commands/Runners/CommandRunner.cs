/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Runtime;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Runners
{
    /// <summary>
    /// The command runner.
    /// </summary>
    public abstract class CommandRunner<TContext, TResult> : ICommandRunner<TContext, TResult> where TContext : CommandRunnerContext where TResult : CommandRunnerResult
    {
        /// <inheritdoc/>
        public abstract Task<TResult> RunAsync(TContext context);

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="terminalLogger">The terminal logger.</param>
        /// <param name="logger">The logger.</param>
        protected CommandRunner(CliOptions options, ITerminalLogger terminalLogger, ILogger logger)
        {
        }
    }
}