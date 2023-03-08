/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Cli.Commands.Runners;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Events;
using PerpetualIntelligence.Cli.Licensing;
using PerpetualIntelligence.Shared.Exceptions;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Handlers
{
    /// <summary>
    /// The default handler to handle a <c>pi-cli</c> command request routed from a <see cref="CommandRouter"/>.
    /// </summary>
    public sealed class CommandHandler : ICommandHandler
    {
        /// <summary>
        /// Initialize a news instance.
        /// </summary>
        public CommandHandler(IServiceProvider services, ILicenseChecker licenseChecker, CliOptions options, ILogger<CommandHandler> logger)
        {
            this.services = services ?? throw new ArgumentNullException(nameof(services));
            this.licenseChecker = licenseChecker ?? throw new ArgumentNullException(nameof(licenseChecker));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<CommandHandlerResult> HandleAsync(CommandHandlerContext context)
        {
            // Optional Event handler
            IAsyncEventHandler? asyncEventHandler = services.GetService<IAsyncEventHandler>();

            // Check the license
            await licenseChecker.CheckAsync(new LicenseCheckerContext(context.License));

            // Checks the command
            CommandCheckerResult checkerResult = await CheckCommandAsync(context, asyncEventHandler);

            // Run the command
            CommandRunnerResult runnerResult = await RunCommandAsync(context, asyncEventHandler);

            // Return the processed result
            return new CommandHandlerResult(runnerResult, checkerResult);
        }

        private async Task<CommandRunnerResult> RunCommandAsync(CommandHandlerContext context, IAsyncEventHandler? asyncEventHandler)
        {
            // Issue a before run event if configured
            if (asyncEventHandler != null)
            {
                await asyncEventHandler.BeforeCommandRunAsync(context.CommandRoute, context.Command);
            }

            // Find the runner and run the command
            IDelegateCommandRunner commandRunner = await FindRunnerOrThrowAsync(context);
            CommandRunnerContext runnerContext = new(context.Command);
            CommandRunnerResult result = await commandRunner.DelegateRunAsync(runnerContext);

            // Process the result, we don't dispose the result here. It is disposed by the routing service at the end.
            await result.ProcessAsync(new(runnerContext));

            // Issue a after run event if configured
            if (asyncEventHandler != null)
            {
                await asyncEventHandler.AfterCommandRunAsync(context.CommandRoute, context.Command, result);
            }

            return result;
        }

        private async Task<CommandCheckerResult> CheckCommandAsync(CommandHandlerContext context, IAsyncEventHandler? asyncEventHandler)
        {
            // Issue a before check event if configured
            if (asyncEventHandler != null)
            {
                await asyncEventHandler.BeforeCommandCheckAsync(context.CommandRoute, context.CommandDescriptor, context.Command);
            }

            // Find the checker and check the command
            ICommandChecker commandChecker = await FindCheckerOrThrowAsync(context);
            var result = await commandChecker.CheckAsync(new CommandCheckerContext(context.CommandDescriptor, context.Command));

            // Issue a after check event if configured
            if (asyncEventHandler != null)
            {
                await asyncEventHandler.AfterCommandCheckAsync(context.CommandRoute, context.CommandDescriptor, context.Command, result);
            }

            return result;
        }

        private Task<ICommandChecker> FindCheckerOrThrowAsync(CommandHandlerContext context)
        {
            // No checker configured.
            if (context.CommandDescriptor.Checker == null)
            {
                throw new ErrorException(Errors.ServerError, "The command checker is not configured. command_name={0} command_id={1}", context.CommandDescriptor.Name, context.CommandDescriptor.Id);
            }

            // Not added to service collection
            object? checkerObj = services.GetService(context.CommandDescriptor.Checker);
            if (checkerObj == null)
            {
                throw new ErrorException(Errors.ServerError, "The command checker is not registered with service collection. command_name={0} command_id={1} checker={2}", context.CommandDescriptor.Name, context.CommandDescriptor.Id, context.CommandDescriptor.Checker.FullName);
            }

            // Invalid checker configured
            if (checkerObj is not ICommandChecker checker)
            {
                throw new ErrorException(Errors.ServerError, "The command checker is not valid. command_name={0} command_id={1} checker={2}", context.CommandDescriptor.Name, context.CommandDescriptor.Id, context.CommandDescriptor.Checker.FullName);
            }

            return Task.FromResult(checker);
        }

        private Task<IDelegateCommandRunner> FindRunnerOrThrowAsync(CommandHandlerContext context)
        {
            // No runner configured.
            if (context.CommandDescriptor.Runner == null)
            {
                throw new ErrorException(Errors.ServerError, "The command runner is not configured. command_name={0} command_id={1}", context.CommandDescriptor.Name, context.CommandDescriptor.Id);
            }

            // Not added to service collection
            object? runnerObj = services.GetService(context.CommandDescriptor.Runner);
            if (runnerObj == null)
            {
                throw new ErrorException(Errors.ServerError, "The command runner is not registered with service collection. command_name={0} command_id={1} runner={2}", context.CommandDescriptor.Name, context.CommandDescriptor.Id, context.CommandDescriptor.Runner.FullName);
            }

            // Invalid runner configured
            if (runnerObj is not IDelegateCommandRunner runnerDelegate)
            {
                throw new ErrorException(Errors.ServerError, "The command runner delegate is not configured. command_name={0} command_id={1} runner={2}", context.CommandDescriptor.Name, context.CommandDescriptor.Id, context.CommandDescriptor.Runner.FullName);
            }

            return Task.FromResult(runnerDelegate);
        }

        private readonly ILicenseChecker licenseChecker;
        private readonly ILogger<CommandHandler> logger;
        private readonly CliOptions options;
        private readonly IServiceProvider services;
    }
}