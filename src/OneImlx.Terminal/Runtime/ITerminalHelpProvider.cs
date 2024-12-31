/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;
using OneImlx.Terminal.Commands.Runners;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// An abstraction to provide help for commands.
    /// </summary>
    /// <remarks>
    /// Use <see cref="ITerminalHelpProvider"/> to provide a common implementation for all your commands. To provide
    /// help for each command individually override <see cref="ICommandRunner{TResult}"/>.
    /// </remarks>
    /// <seealso cref="IDelegateCommandRunner"/>
    /// <seealso cref="ICommandRunner{TResult}"/>
    /// <seealso cref="CommandRunner{TResult}"/>
    public interface ITerminalHelpProvider
    {
        /// <summary>
        /// Provides help asynchronously.
        /// </summary>
        /// <returns></returns>
        public Task ProvideHelpAsync(TerminalHelpProviderContext context);
    }
}
