/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Cli.Commands.Runners;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Oidc;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Extensions;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Handlers
{
    /// <summary>
    /// The command handler to handle a <c>cli</c> command request routed from a <see cref="CommandRouter"/>.
    /// </summary>
    public class CommandHandler : ICommandHandler
    {
        /// <summary>
        /// Initialize a news instance.
        /// </summary>
        public CommandHandler(IServiceProvider services, CliOptions options, ILogger<CommandHandler> logger)
        {
            this.services = services ?? throw new ArgumentNullException(nameof(services));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public virtual async Task<CommandHandlerResult> HandleAsync(CommandHandlerContext context)
        {
            // Find the checker and check the command
            ICommandChecker commandChecker = await FindCheckerOrThrowAsync(context);
            await commandChecker.CheckAsync(new CommandCheckerContext(context.CommandIdentity, context.Command));

            // Find the runner and run the command
            ICommandRunner commandRunner = await FindRunnerOrThrowAsync(context);
            await commandRunner.RunAsync(new CommandRunnerContext(context.Command));

            // Return the result to process it further.
            return new CommandHandlerResult();
        }

        private Task<ICommandChecker> FindCheckerOrThrowAsync(CommandHandlerContext context)
        {
            // No checker configured.
            if (context.CommandIdentity.Checker == null)
            {
                throw new ErrorException(Errors.ServerError, "The command checker is not configured. command_name={0} command_id={1}", context.CommandIdentity.Name, context.CommandIdentity.Id);
            }

            // Not added to service collection
            object? checkerObj = services.GetService(context.CommandIdentity.Checker);
            if (checkerObj == null)
            {
                throw new ErrorException(Errors.ServerError, "The command checker is not registered with service collection. command_name={0} command_id={1} checker={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, context.CommandIdentity.Checker.FullName);
            }

            // Invalid checker configured
            if (checkerObj is not ICommandChecker checker)
            {
                throw new ErrorException(Errors.ServerError, "The command checker is not valid. command_name={0} command_id={1} checker={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, context.CommandIdentity.Checker.FullName);
            }

            logger.FormatAndLog(LogLevel.Debug, options.Logging, "The handler found a command checker. command_name={0} command_id={1} checker={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, checker.GetType().FullName);
            return Task.FromResult(checker);
        }

        private Task<ICommandRunner> FindRunnerOrThrowAsync(CommandHandlerContext context)
        {
            // No runner configured.
            if (context.CommandIdentity.Runner == null)
            {
                throw new ErrorException(Errors.ServerError, "The command runner is not configured. command_name={0} command_id={1}", context.CommandIdentity.Name, context.CommandIdentity.Id);
            }

            // Not added to service collection
            object? runnerObj = services.GetService(context.CommandIdentity.Runner);
            if (runnerObj == null)
            {
                throw new ErrorException(Errors.ServerError, "The command runner is not registered with service collection. command_name={0} command_id={1} runner={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, context.CommandIdentity.Runner.FullName);
            }

            // Invalid runner configured
            if (runnerObj is not ICommandRunner runner)
            {
                throw new ErrorException(Errors.ServerError, "The command runner is not valid. command_name={0} command_id={1} runner={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, context.CommandIdentity.Runner.FullName);
            }

            logger.FormatAndLog(LogLevel.Debug, options.Logging, "The handler found a command runner. command_name={0} command_id={1} runner={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, runner.GetType().FullName);
            return Task.FromResult(runner);
        }

        private readonly ILogger<CommandHandler> logger;
        private readonly CliOptions options;
        private readonly IServiceProvider services;
    }
}
