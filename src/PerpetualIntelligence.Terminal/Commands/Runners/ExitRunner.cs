/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Hosting;
using PerpetualIntelligence.Terminal.Extensions;
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
        public ExitRunner(IHost host)
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