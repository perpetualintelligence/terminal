/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The default command extractor.
    /// </summary>
    public class CommandChecker : ICommandChecker
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public CommandChecker(CliOptions options, ILogger<CommandChecker> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        public Task<CommandCheckerResult> CheckAsync(CommandCheckerContext context)
        {
            return Task.FromResult(new CommandCheckerResult());
        }

        private readonly ILogger<CommandChecker> logger;
        private readonly CliOptions options;
    }
}
