/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Providers;
using PerpetualIntelligence.Shared.Exceptions;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Runners
{
    /// <summary>
    /// The command runner.
    /// </summary>
    public abstract class CommandRunner<TResult> : IDelegateCommandRunner, ICommandRunner<TResult> where TResult : CommandRunnerResult
    {
        private IHelpProvider? helpProvider;

        /// <inheritdoc/>
        public async Task<CommandRunnerResult> DelegateHelpAsync(CommandRunnerContext context, IHelpProvider helpProvider)
        {
            this.helpProvider = helpProvider;
            await HelpAsync(context);
            return CommandRunnerResult.NoProcessing;
        }

        /// <inheritdoc/>
        public async Task<CommandRunnerResult> DelegateRunAsync(CommandRunnerContext context)
        {
            var result = await RunAsync(context);
            return (CommandRunnerResult)(object)result;
        }

        /// <summary>
        /// Runs the command help asynchronously.
        /// </summary>
        /// <param name="context">The command context.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual Task HelpAsync(CommandRunnerContext context)
        {
            if (helpProvider == null)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The help provider is missing in the configured services.");
            }

            return helpProvider.ProvideHelpAsync(new HelpProviderContext(context.Command));
        }

        /// <inheritdoc/>
        public abstract Task<TResult> RunAsync(CommandRunnerContext context);
    }
}