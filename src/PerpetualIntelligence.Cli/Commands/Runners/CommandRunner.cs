/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Runners
{
    /// <summary>
    /// The command runner.
    /// </summary>
    public abstract class CommandRunner : ICommandRunner
    {
        /// <inheritdoc/>
        public abstract Task<CommandRunnerResult> RunAsync(CommandRunnerContext context);

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        protected CommandRunner(CliOptions options, ILogger logger)
        {
            this.logger = logger;
            this.options = options;
        }

        /// <summary>
        /// The logger.
        /// </summary>
        protected ILogger logger;

        /// <summary>
        /// The configuration options.
        /// </summary>
        protected CliOptions options;
    }
}
