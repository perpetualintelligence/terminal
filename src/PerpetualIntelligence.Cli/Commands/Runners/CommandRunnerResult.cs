/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Runners
{
    /// <summary>
    /// The command runner result.
    /// </summary>
    public class CommandRunnerResult : ICommandRunnerResult
    {
        /// <summary>
        /// Creates a default runner result that does not perform any additional processing.
        /// </summary>
        public static CommandRunnerResult NoProcessing => new();

        /// <summary>
        /// Disposes the managed resources.
        /// </summary>
        public virtual ValueTask DisposeAsync()
        {
            return new ValueTask();
        }

        /// <summary>
        /// Processes the runner result asynchronously.
        /// </summary>
        /// <param name="context">The runner result context.</param>
        /// <returns>An asynchronous task.</returns>
        public virtual Task ProcessAsync(CommandRunnerResultProcessorContext context)
        {
            return Task.CompletedTask;
        }
    }
}
