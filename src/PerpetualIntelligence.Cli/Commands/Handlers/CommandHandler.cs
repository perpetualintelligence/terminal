/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands.Checkers;
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
    /// The <c>oneimlx</c> generic command handler.
    /// </summary>
    public class CommandHandler : ICommandHandler
    {
        /// <summary>
        /// Initialize a news instance.
        /// </summary>
        public CommandHandler(CliOptions options, ILogger<CommandHandler> logger)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public virtual async Task<CommandHandlerResult> HandleAsync(CommandHandlerContext context)
        {
            CommandHandlerResult result = new();

            // Find the checker
            OneImlxTryResult<CommandChecker> commandChecker = await TryFindCheckerAsync(context);
            if (commandChecker.IsError)
            {
                result.SyncError(commandChecker);
                return result;
            }

            // Check the command
            var checkerResult = await commandChecker.Result.CheckAsync(new CommandCheckerContext(context.CommandIdentity, context.Command));
            if (checkerResult.IsError)
            {
                result.SyncError(checkerResult);
                return result;
            }

            // Find the runner
            OneImlxTryResult<CommandRunner> commandRunner = await TryFindRunnerAsync(context);
            if (commandRunner.IsError)
            {
                result.SyncError(commandRunner);
                return result;
            }

            // Run the command
            CommandRunnerResult runnerResult = await commandRunner.Result.RunAsync(new CommandRunnerContext(context.Command));
            if (runnerResult.IsError)
            {
                result.SyncError(runnerResult);
                return result;
            }

            // Return the result to process it further.
            return result;
        }

        private Task<OneImlxTryResult<CommandChecker>> TryFindCheckerAsync(CommandHandlerContext context)
        {
            if (context.Services.GetService(context.CommandIdentity.Checker) is CommandChecker checker)
            {
                logger.LogDebug("The handler found a command checker. command_name={0} command_id={1} checker={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, checker.GetType().FullName);
                return Task.FromResult<OneImlxTryResult<CommandChecker>>(new(checker));
            }
            else
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The command checker is not configured. command_name={0} command_id={1}", context.CommandIdentity.Name, context.CommandIdentity.Id);
                return Task.FromResult(OneImlxResult.NewError<OneImlxTryResult<CommandChecker>>(Errors.ServerError, errorDesc));
            }
        }

        private Task<OneImlxTryResult<CommandRunner>> TryFindRunnerAsync(CommandHandlerContext context)
        {
            if (context.Services.GetService(context.CommandIdentity.Runner) is CommandRunner runner)
            {
                logger.LogDebug("The handler found a command runner. command_name={0} command_id={1} runner={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, runner.GetType().FullName);
                return Task.FromResult<OneImlxTryResult<CommandRunner>>(new(runner));
            }
            else
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The command checker is not configured. command_name={0} command_id={1}", context.CommandIdentity.Name, context.CommandIdentity.Id);
                return Task.FromResult(OneImlxResult.NewError<OneImlxTryResult<CommandRunner>>(Errors.ServerError, errorDesc));
            }
        }

        private readonly ILogger<CommandHandler> logger;
        private readonly CliOptions options;
    }
}
