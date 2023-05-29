/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Runners
{
    /// <summary>
    /// An abstraction of a command runner.
    /// </summary>
    public interface ICommandRunner<TResult> where TResult : CommandRunnerResult
    {
        /// <summary>
        /// Runs a command asynchronously.
        /// </summary>
        /// <param name="context">The runner context.</param>
        /// <returns>The runner result.</returns>
        Task<TResult> RunAsync(CommandRunnerContext context);

        /// <summary>
        /// Runs a command help asynchronously.
        /// </summary>
        /// <param name="context">The runner context.</param>
        /// <returns>The runner result.</returns>
        Task HelpAsync(CommandRunnerContext context);
    }
}