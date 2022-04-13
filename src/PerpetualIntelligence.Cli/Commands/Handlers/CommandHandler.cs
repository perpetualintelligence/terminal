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
using PerpetualIntelligence.Cli.Licensing;
using PerpetualIntelligence.Shared.Exceptions;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Handlers
{
    /// <summary>
    /// The default handler to handle a <c>cli</c> command request routed from a <see cref="CommandRouter"/>.
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
            // Check the license
            await licenseChecker.CheckAsync(new LicenseCheckerContext(context.License));

            // Find the checker and check the command
            ICommandChecker commandChecker = await FindCheckerOrThrowAsync(context);
            await commandChecker.CheckAsync(new CommandCheckerContext(context.CommandDescriptor, context.Command));

            // Find the runner and run the command
            ICommandRunner commandRunner = await FindRunnerOrThrowAsync(context);
            await commandRunner.RunAsync(new CommandRunnerContext(context.Command));

            // Return the result to process it further.
            return new CommandHandlerResult();
        }

        private Task<ICommandChecker> FindCheckerOrThrowAsync(CommandHandlerContext context)
        {
            // No checker configured.
            if (context.CommandDescriptor._checker == null)
            {
                throw new ErrorException(Errors.ServerError, "The command checker is not configured. command_name={0} command_id={1}", context.CommandDescriptor.Name, context.CommandDescriptor.Id);
            }

            // Not added to service collection
            object? checkerObj = services.GetService(context.CommandDescriptor._checker);
            if (checkerObj == null)
            {
                throw new ErrorException(Errors.ServerError, "The command checker is not registered with service collection. command_name={0} command_id={1} checker={2}", context.CommandDescriptor.Name, context.CommandDescriptor.Id, context.CommandDescriptor._checker.FullName);
            }

            // Invalid checker configured
            if (checkerObj is not ICommandChecker checker)
            {
                throw new ErrorException(Errors.ServerError, "The command checker is not valid. command_name={0} command_id={1} checker={2}", context.CommandDescriptor.Name, context.CommandDescriptor.Id, context.CommandDescriptor._checker.FullName);
            }

            return Task.FromResult(checker);
        }

        private Task<ICommandRunner> FindRunnerOrThrowAsync(CommandHandlerContext context)
        {
            // No runner configured.
            if (context.CommandDescriptor._runner == null)
            {
                throw new ErrorException(Errors.ServerError, "The command runner is not configured. command_name={0} command_id={1}", context.CommandDescriptor.Name, context.CommandDescriptor.Id);
            }

            // Not added to service collection
            object? runnerObj = services.GetService(context.CommandDescriptor._runner);
            if (runnerObj == null)
            {
                throw new ErrorException(Errors.ServerError, "The command runner is not registered with service collection. command_name={0} command_id={1} runner={2}", context.CommandDescriptor.Name, context.CommandDescriptor.Id, context.CommandDescriptor._runner.FullName);
            }

            // Invalid runner configured
            if (runnerObj is not ICommandRunner runner)
            {
                throw new ErrorException(Errors.ServerError, "The command runner is not valid. command_name={0} command_id={1} runner={2}", context.CommandDescriptor.Name, context.CommandDescriptor.Id, context.CommandDescriptor._runner.FullName);
            }

            return Task.FromResult(runner);
        }

        private readonly ILicenseChecker licenseChecker;
        private readonly ILogger<CommandHandler> logger;
        private readonly CliOptions options;
        private readonly IServiceProvider services;
    }
}
