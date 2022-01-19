/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Cli.Commands.Runners;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Oidc;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Infrastructure;
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
            CommandHandlerResult result = new();

            // Find the checker
            TryResult<ICommandChecker> commandChecker = await TryFindCheckerAsync(context);
            if (commandChecker.IsError)
            {
                result.SyncError(commandChecker);
                return result;
            }

            // Check the command, result will not be null here we already checked it in TryFindCheckerAsync.
            var checkerResult = await commandChecker.Result!.CheckAsync(new CommandCheckerContext(context.CommandIdentity, context.Command));
            if (checkerResult.IsError)
            {
                result.SyncError(checkerResult);
                return result;
            }

            // Find the runner
            TryResult<ICommandRunner> commandRunner = await TryFindRunnerAsync(context);
            if (commandRunner.IsError)
            {
                result.SyncError(commandRunner);
                return result;
            }

            // Run the command, result will not be null here we already checked it in TryFindRunnerAsync.
            CommandRunnerResult runnerResult = await commandRunner.Result!.RunAsync(new CommandRunnerContext(context.Command));
            if (runnerResult.IsError)
            {
                result.SyncError(runnerResult);
                return result;
            }

            // Return the result to process it further.
            return result;
        }

        private Task<TryResult<ICommandChecker>> TryFindCheckerAsync(CommandHandlerContext context)
        {
            // No checker configured.
            if (context.CommandIdentity.Checker == null)
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The command checker is not configured. command_name={0} command_id={1}", context.CommandIdentity.Name, context.CommandIdentity.Id);
                return Task.FromResult(Result.NewError<TryResult<ICommandChecker>>(Errors.ServerError, errorDesc));
            }

            // Not added to service collection
            object? checkerObj = services.GetService(context.CommandIdentity.Checker);
            if (checkerObj == null)
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The command checker is not registered with service collection. command_name={0} command_id={1} checker={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, context.CommandIdentity.Checker.FullName);
                return Task.FromResult(Result.NewError<TryResult<ICommandChecker>>(Errors.ServerError, errorDesc));
            }

            // Invalid checker configured
            if (checkerObj is not ICommandChecker checker)
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The command checker is not valid. command_name={0} command_id={1} checker={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, context.CommandIdentity.Checker.FullName);
                return Task.FromResult(Result.NewError<TryResult<ICommandChecker>>(Errors.ServerError, errorDesc));
            }

            logger.FormatAndLog(LogLevel.Debug, options.Logging, "The handler found a command checker. command_name={0} command_id={1} checker={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, checker.GetType().FullName);
            return Task.FromResult<TryResult<ICommandChecker>>(new(checker));
        }

        private Task<TryResult<ICommandRunner>> TryFindRunnerAsync(CommandHandlerContext context)
        {
            // No runner configured.
            if (context.CommandIdentity.Runner == null)
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The command runner is not configured. command_name={0} command_id={1}", context.CommandIdentity.Name, context.CommandIdentity.Id);
                return Task.FromResult(Result.NewError<TryResult<ICommandRunner>>(Errors.ServerError, errorDesc));
            }

            // Not added to service collection
            object? runnerObj = services.GetService(context.CommandIdentity.Runner);
            if (runnerObj == null)
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The command runner is not registered with service collection. command_name={0} command_id={1} runner={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, context.CommandIdentity.Runner.FullName);
                return Task.FromResult(Result.NewError<TryResult<ICommandRunner>>(Errors.ServerError, errorDesc));
            }

            // Invalid runner configured
            if (runnerObj is not ICommandRunner runner)
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The command runner is not valid. command_name={0} command_id={1} runner={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, context.CommandIdentity.Runner.FullName);
                return Task.FromResult(Result.NewError<TryResult<ICommandRunner>>(Errors.ServerError, errorDesc));
            }

            logger.FormatAndLog(LogLevel.Debug, options.Logging, "The handler found a command runner. command_name={0} command_id={1} runner={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, runner.GetType().FullName);
            return Task.FromResult<TryResult<ICommandRunner>>(new(runner));
        }

        private readonly ILogger<CommandHandler> logger;
        private readonly CliOptions options;
        private readonly IServiceProvider services;
    }
}
