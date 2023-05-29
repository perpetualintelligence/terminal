/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Runners;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Providers
{
    /// <summary>
    /// An abstraction to provide help for commands.
    /// </summary>
    /// <remarks>
    /// Use <see cref="IHelpProvider"/> to provide a common implementation for all your commands. To provide help for each command individually override <see cref="ICommandRunner{TResult}.HelpAsync(CommandRunnerContext)"/>.
    /// </remarks>
    /// <seealso cref="IDelegateCommandRunner"/>
    /// <seealso cref="ICommandRunner{TResult}"/>
    /// <seealso cref="CommandRunner{TResult}"/>
    public interface IHelpProvider
    {
        /// <summary>
        /// Provides help asynchronously.
        /// </summary>
        /// <returns></returns>
        public Task ProvideHelpAsync(HelpProviderContext context);
    }
}