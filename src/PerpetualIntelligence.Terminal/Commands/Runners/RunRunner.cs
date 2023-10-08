/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Configuration.Options;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Runners
{
    /// <summary>
    /// Runs an OS command. This runner will be available in a future release. It does not perform any action right now.
    /// For more information on road-map, <see href="https://docs.perpetualintelligence.com/articles/pi-cli/roadmap.html"/>
    /// </summary>
    public class RunRunner : CommandRunner<CommandRunnerResult>
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        public RunRunner(TerminalOptions options)
        {
        }

        /// <inheritdoc/>
        public override Task<CommandRunnerResult> RunCommandAsync(CommandRunnerContext context)
        {
            // TODO;
            return Task.FromResult(new CommandRunnerResult());
        }
    }
}