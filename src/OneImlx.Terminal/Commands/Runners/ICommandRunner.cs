/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;
using OneImlx.Terminal.Commands.Routers;

namespace OneImlx.Terminal.Commands.Runners
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
        Task<TResult> RunCommandAsync(CommandRouterContext context);

        /// <summary>
        /// Runs a command help asynchronously.
        /// </summary>
        /// <param name="context">The runner context.</param>
        /// <returns>The runner result.</returns>
        Task RunHelpAsync(CommandRouterContext context);
    }
}
