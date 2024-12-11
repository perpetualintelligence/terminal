/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Runners
{
    /// <summary>
    /// An abstraction to delegate to <see cref="ICommandRunner{TResult}.RunCommandAsync(CommandRunnerContext)"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="IDelegateCommandRunner"/> enables the use of generics with <see cref="ICommandRunner{TResult}"/>.
    /// All implementations must delegate by calling <see cref="ICommandRunner{TResult}.RunCommandAsync(CommandRunnerContext)"/> without any business logic.
    /// </remarks>
    public interface IDelegateCommandRunner
    {
        /// <summary>
        /// Delegates to <see cref="ICommandRunner{TResult}.RunCommandAsync(CommandRouterContext)"/> asynchronously.
        /// </summary>
        /// <param name="context">The runner context.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>The runner result.</returns>

        Task<CommandRunnerResult> DelegateRunAsync(CommandRouterContext context, ILogger? logger = null);

        /// <summary>
        /// Delegates to <see cref="ICommandRunner{TResult}.RunHelpAsync(CommandRouterContext)"/> asynchronously.
        /// </summary>
        /// <param name="context">The runner context.</param>
        /// <param name="helpProvider">The help provider.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>The runner result.</returns>
        Task<CommandRunnerResult> DelegateHelpAsync(CommandRouterContext context, ITerminalHelpProvider helpProvider, ILogger? logger = null);
    }
}