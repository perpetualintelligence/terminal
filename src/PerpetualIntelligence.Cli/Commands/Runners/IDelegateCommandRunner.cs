/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Providers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Runners
{
    /// <summary>
    /// An abstraction to delegate to <see cref="ICommandRunner{TResult}.RunAsync(CommandRunnerContext)"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="IDelegateCommandRunner"/> enables the use of generics with <see cref="ICommandRunner{TResult}"/>.
    /// All implementations must delegate by calling <see cref="ICommandRunner{TResult}.RunAsync(CommandRunnerContext)"/> without any business logic.
    /// </remarks>
    public interface IDelegateCommandRunner
    {
        /// <summary>
        /// Delegates to <see cref="ICommandRunner{TResult}.RunAsync(CommandRunnerContext)"/> asynchronously.
        /// </summary>
        /// <param name="context">The runner context.</param>
        /// <returns>The runner result.</returns>

        Task<CommandRunnerResult> DelegateRunAsync(CommandRunnerContext context);

        /// <summary>
        /// Delegates to <see cref="ICommandRunner{TResult}.HelpAsync(CommandRunnerContext)"/> asynchronously.
        /// </summary>
        /// <param name="context">The runner context.</param>
        /// <param name="helpProvider">The help provider.</param>
        /// <returns>The runner result.</returns>
        Task<CommandRunnerResult> DelegateHelpAsync(CommandRunnerContext context, IHelpProvider helpProvider);
    }
}