/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Commands.Runners
{
    /// <summary>
    /// An abstraction to delegate to <see cref="ICommandRunner{TResult}"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="IDelegateCommandRunner"/> enables the use of generics with <see cref="ICommandRunner{TResult}"/>.
    /// All implementations must delegate by calling <see cref="ICommandRunner{TResult}"/> without any business logic.
    /// </remarks>
    public interface IDelegateCommandRunner
    {
        /// <summary>
        /// Delegates to <see cref="ICommandRunner{TResult}.RunHelpAsync(CommandContext)"/> asynchronously.
        /// </summary>
        /// <param name="context">The runner context.</param>
        /// <param name="helpProvider">The help provider.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>The runner result.</returns>
        Task<CommandRunnerResult> DelegateHelpAsync(CommandContext context, ITerminalHelpProvider helpProvider, ILogger? logger = null);

        /// <summary>
        /// Delegates to <see cref="ICommandRunner{TResult}.RunCommandAsync(CommandContext)"/> asynchronously.
        /// </summary>
        /// <param name="context">The runner context.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>The runner result.</returns>
        Task<CommandRunnerResult> DelegateRunAsync(CommandContext context, ILogger? logger = null);
    }
}
