/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands.Runners;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Extensions;
using System.Threading;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Defaults
{
    /// <summary>
    /// The <c>urn:oneimlx:cli:exit</c> command runner.
    /// </summary>
    public class ExitRunner : CommandRunner
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public ExitRunner(IHost host, CliOptions options, ILogger<ExitRunner> logger) : base(options, logger)
        {
            this.host = host;
        }

        /// <inheritdoc/>
        public override async Task<CommandRunnerResult> RunAsync(CommandRunnerContext context)
        {
            // Confirm
            string? answer = await context.Command.ReadAnswerAsync("Are you sure", "y", "n");
            if (answer == "y" || answer == "Y")
            {
                await host.StopAsync(CancellationToken.None);
            }

            return new CommandRunnerResult();
        }

        private readonly IHost host;
    }
}
