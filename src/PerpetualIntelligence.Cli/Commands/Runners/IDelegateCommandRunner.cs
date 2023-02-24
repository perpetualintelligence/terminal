/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Runners
{
    /// <summary>
    /// An abstraction to delegate to <see cref="ICommandRunner{TResult}.RunAsync(CommandRunnerContext)"/>.
    /// </summary>
    public interface IDelegateCommandRunner
    {
        /// <summary>
        /// Delegates to <see cref="ICommandRunner{TResult}.RunAsync(CommandRunnerContext)"/> asynchronously.
        /// </summary>
        /// <param name="context">The runner context.</param>
        /// <returns>The runner result.</returns>
        /// <remarks>
        /// The <see cref="IDelegateCommandRunner"/> enables the use of generics with <see cref="ICommandRunner{TResult}"/>.
        /// All implementations must delegate by calling <see cref="ICommandRunner{TResult}.RunAsync(CommandRunnerContext)"/> without any business logic.
        /// </remarks>
        Task<CommandRunnerResult> DelegateRunAsync(CommandRunnerContext context);
    }
}