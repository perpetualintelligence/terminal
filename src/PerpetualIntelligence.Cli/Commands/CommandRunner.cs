/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The default command runner.
    /// </summary>
    public class CommandRunner : ICommandRunner
    {
        /// <summary>
        /// Initialize a news instance.
        /// </summary>
        public CommandRunner(CliOptions options, ILogger<CommandRunner> logger)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public Task<CommandRunnerResult> RunAsync(CommandRunnerContext context)
        {
            CommandRunnerResult result = new();
            return Task.FromResult(result);
        }

        private readonly ILogger<CommandRunner> logger;
        private readonly CliOptions options;
    }
}
