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
    /// The command runner is where developers implement how commands are executed. It operates asynchronously to handle
    /// commands that might take a while to process. The framework routes each parsed command to its specific runner,
    /// helping to organize and manage the command execution logic within your application.
    /// </summary>
    public abstract class CommandRunner<TResult> : IDelegateCommandRunner, ICommandRunner<TResult> where TResult : CommandRunnerResult
    {
        /// <inheritdoc/>
        public async Task<CommandRunnerResult> DelegateHelpAsync(CommandContext context, ITerminalHelpProvider helpProvider, ILogger? logger = null)
        {
            this.helpProvider = helpProvider;

            this.logger = logger;
            logger?.LogDebug("Run help. command={0}", context.EnsureParsedCommand().Command.Id);

            await RunHelpAsync(context);
            return new CommandRunnerResult();
        }

        /// <inheritdoc/>
        public async Task<CommandRunnerResult> DelegateRunAsync(CommandContext context, ILogger? logger = null)
        {
            this.logger = logger;
            logger?.LogDebug("Run command. command={0}", context.EnsureParsedCommand().Command.Id);

            var result = await RunCommandAsync(context);
            return (CommandRunnerResult)(object)result;
        }

        /// <inheritdoc/>
        public abstract Task<TResult> RunCommandAsync(CommandContext context);

        /// <summary>
        /// Runs the command help asynchronously.
        /// </summary>
        /// <param name="context">The command context.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual Task RunHelpAsync(CommandContext context)
        {
            if (helpProvider == null)
            {
                throw new TerminalException(TerminalErrors.InvalidConfiguration, "The help provider is missing in the configured services.");
            }

            return helpProvider.ProvideHelpAsync(new TerminalHelpProviderContext(context.EnsureParsedCommand().Command));
        }

        private ITerminalHelpProvider? helpProvider;
        private ILogger? logger;
    }
}
