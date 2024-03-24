/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands.Runners;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// An abstraction to provide help for commands.
    /// </summary>
    /// <remarks>
    /// Use <see cref="ITerminalHelpProvider"/> to provide a common implementation for all your commands. To provide help for each command individually override <see cref="ICommandRunner{TResult}.RunHelpAsync(CommandRunnerContext)"/>.
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