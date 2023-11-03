/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Terminal.Commands.Providers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Runners
{
    /// <summary>
    /// The command runner is where developers implement how commands are executed. It operates asynchronously to handle commands that might take a while to process.
    /// The framework routes each parsed command to its specific runner, helping to organize and manage the command execution logic within your application.
    /// </summary>
    public abstract class CommandRunner<TResult> : IDelegateCommandRunner, ICommandRunner<TResult> where TResult : CommandRunnerResult
    {
        private IHelpProvider? helpProvider;
        private ILogger? logger;

        /// <inheritdoc/>
        public async Task<CommandRunnerResult> DelegateHelpAsync(CommandRunnerContext context, IHelpProvider helpProvider, ILogger? logger = null)
        {
            this.helpProvider = helpProvider;

            this.logger = logger;
            logger?.LogDebug("Run help. command={0}", context.HandlerContext.ParsedCommand.Command.Id);

            await RunHelpAsync(context);
            return CommandRunnerResult.NoProcessing;
        }

        /// <inheritdoc/>
        public async Task<CommandRunnerResult> DelegateRunAsync(CommandRunnerContext context, ILogger? logger = null)
        {
            this.logger = logger;
            logger?.LogDebug("Run command. command={0}", context.HandlerContext.ParsedCommand.Command.Id);

            var result = await RunCommandAsync(context);
            return (CommandRunnerResult)(object)result;
        }

        /// <summary>
        /// Runs the command help asynchronously.
        /// </summary>
        /// <param name="context">The command context.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual Task RunHelpAsync(CommandRunnerContext context)
        {
            if (helpProvider == null)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The help provider is missing in the configured services.");
            }

            return helpProvider.ProvideHelpAsync(new HelpProviderContext(context.HandlerContext.ParsedCommand.Command));
        }

        /// <inheritdoc/>
        public abstract Task<TResult> RunCommandAsync(CommandRunnerContext context);
    }
}