/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Events;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Runtime;
using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Handlers
{
    /// <summary>
    /// The default handler to handle a command request routed from a <see cref="CommandRouter"/>.
    /// </summary>
    public sealed class CommandHandler : ICommandHandler
    {
        /// <summary>
        /// Initialize a news instance.
        /// </summary>
        public CommandHandler(IServiceProvider services, ILicenseChecker licenseChecker, TerminalOptions options, ILogger<CommandHandler> logger)
        {
            this.services = services ?? throw new ArgumentNullException(nameof(services));
            this.licenseChecker = licenseChecker ?? throw new ArgumentNullException(nameof(licenseChecker));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<CommandHandlerResult> HandleCommandAsync(CommandHandlerContext context)
        {
            logger.LogDebug("Handle route. route={0}", context.RouterContext.Route.Id);

            // Check the license
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(context.License));

            // Check and run the command
            ITerminalEventHandler? asyncEventHandler = services.GetService<ITerminalEventHandler>();
            Tuple<CommandCheckerResult, CommandRunnerResult> result = await CheckAndRunCommandInnerAsync(context, asyncEventHandler);

            // Return the processed result
            return new CommandHandlerResult(result.Item2, result.Item1);
        }

        private async Task<Tuple<CommandCheckerResult, CommandRunnerResult>> CheckAndRunCommandInnerAsync(CommandHandlerContext context, ITerminalEventHandler? asyncEventHandler)
        {
            // If we are executing a help command then we need to bypass all the checks.
            if (!options.Help.Disabled.GetValueOrDefault() &&
                (context.ParsedCommand.Command.TryGetOption(options.Help.OptionId, out Option? helpOption) ||
                 context.ParsedCommand.Command.TryGetOption(options.Help.OptionAlias, out helpOption)
                ))
            {
                logger.LogDebug("Found help option. option={0}", helpOption != null ? helpOption.Id : "?");
                CommandRunnerResult runnerResult = await RunCommandInnerAsync(context, runHelp: true, asyncEventHandler);
                return new Tuple<CommandCheckerResult, CommandRunnerResult>(new CommandCheckerResult(), runnerResult);
            }
            else
            {
                CommandCheckerResult checkerResult = await CheckCommandInnerAsync(context, asyncEventHandler);
                CommandRunnerResult runnerResult = await RunCommandInnerAsync(context, runHelp: false, asyncEventHandler);
                return new Tuple<CommandCheckerResult, CommandRunnerResult>(checkerResult, runnerResult);
            }
        }

        private async Task<CommandRunnerResult> RunCommandInnerAsync(CommandHandlerContext context, bool runHelp, ITerminalEventHandler? asyncEventHandler)
        {
            // Issue a before run event if configured
            if (asyncEventHandler != null)
            {
                logger.LogDebug("Fire event. event={0} command={1}", nameof(asyncEventHandler.BeforeCommandRunAsync), context.ParsedCommand.Command.Id);
                await asyncEventHandler.BeforeCommandRunAsync(context.ParsedCommand.Command);
            }

            // Find the runner to run the command
            IDelegateCommandRunner commandRunner = await FindRunnerOrThrowAsync(context);
            CommandRunnerContext runnerContext = new(context);
            CommandRunnerResult runnerResult;

            // Run or Help
            if (runHelp)
            {                
                ITerminalHelpProvider helpProvider = services.GetRequiredService<ITerminalHelpProvider>();
                logger.LogDebug("Skip runner. Delegate to help provider. type={0}", helpProvider.GetType().Name);
                runnerResult = await commandRunner.DelegateHelpAsync(runnerContext, helpProvider, logger);
            }
            else
            {
                runnerResult = await commandRunner.DelegateRunAsync(runnerContext, logger);
            }

            // Process the result.
            await runnerResult.ProcessAsync(runnerContext, logger);

            // Issue a after run event if configured
            if (asyncEventHandler != null)
            {
                logger.LogDebug("Fire event. event={0} command={1}", nameof(asyncEventHandler.AfterCommandRunAsync), context.ParsedCommand.Command.Id);
                await asyncEventHandler.AfterCommandRunAsync(context.ParsedCommand.Command, runnerResult);
            }

            // Dispose the result.
            await runnerResult.DisposeAsync();

            return runnerResult;
        }

        private async Task<CommandCheckerResult> CheckCommandInnerAsync(CommandHandlerContext context, ITerminalEventHandler? asyncEventHandler)
        {
            // Issue a before check event if configured
            if (asyncEventHandler != null)
            {
                logger.LogDebug("Fire event. event={0} command={1}", nameof(asyncEventHandler.BeforeCommandCheckAsync), context.ParsedCommand.Command.Id);
                await asyncEventHandler.BeforeCommandCheckAsync(context.ParsedCommand.Command);
            }

            // Find the checker and check the command
            ICommandChecker commandChecker = await FindCheckerOrThrowAsync(context);
            var result = await commandChecker.CheckCommandAsync(new CommandCheckerContext(context));

            // Issue a after check event if configured
            if (asyncEventHandler != null)
            {
                logger.LogDebug("Fire event. event={0} command={1}", nameof(asyncEventHandler.AfterCommandCheckAsync), context.ParsedCommand.Command.Id);
                await asyncEventHandler.AfterCommandCheckAsync(context.ParsedCommand.Command, result);
            }

            return result;
        }

        private Task<ICommandChecker> FindCheckerOrThrowAsync(CommandHandlerContext context)
        {
            // No checker configured.
            if (context.ParsedCommand.Command.Descriptor.Checker == null)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The command checker is not configured. command_name={0} command_id={1}", context.ParsedCommand.Command.Descriptor.Name, context.ParsedCommand.Command.Descriptor.Id);
            }

            // Not added to service collection
            object? checkerObj = services.GetService(context.ParsedCommand.Command.Descriptor.Checker) ?? throw new TerminalException(TerminalErrors.ServerError, "The command checker is not registered with service collection. command_name={0} command_id={1} checker={2}", context.ParsedCommand.Command.Descriptor.Name, context.ParsedCommand.Command.Descriptor.Id, context.ParsedCommand.Command.Descriptor.Checker.Name);

            // Invalid checker configured
            if (checkerObj is not ICommandChecker checker)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The command checker is not valid. command_name={0} command_id={1} checker={2}", context.ParsedCommand.Command.Descriptor.Name, context.ParsedCommand.Command.Descriptor.Id, context.ParsedCommand.Command.Descriptor.Checker.Name);
            }

            logger.LogDebug("Found checker. type={0}", checker.GetType().Name);
            return Task.FromResult(checker);
        }

        private Task<IDelegateCommandRunner> FindRunnerOrThrowAsync(CommandHandlerContext context)
        {
            // No runner configured.
            if (context.ParsedCommand.Command.Descriptor.Runner == null)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The command runner is not configured. command_name={0} command_id={1}", context.ParsedCommand.Command.Descriptor.Name, context.ParsedCommand.Command.Descriptor.Id);
            }

            // Not added to service collection
            object? runnerObj = services.GetService(context.ParsedCommand.Command.Descriptor.Runner) ?? throw new TerminalException(TerminalErrors.ServerError, "The command runner is not registered with service collection. command_name={0} command_id={1} runner={2}", context.ParsedCommand.Command.Descriptor.Name, context.ParsedCommand.Command.Descriptor.Id, context.ParsedCommand.Command.Descriptor.Runner.Name);

            // Invalid runner configured
            if (runnerObj is not IDelegateCommandRunner runnerDelegate)
            {
                throw new TerminalException(TerminalErrors.ServerError, "The command runner delegate is not configured. command_name={0} command_id={1} runner={2}", context.ParsedCommand.Command.Descriptor.Name, context.ParsedCommand.Command.Descriptor.Id, context.ParsedCommand.Command.Descriptor.Runner.Name);
            }

            logger.LogDebug("Found runner. type={0}", runnerDelegate.GetType().Name);
            return Task.FromResult(runnerDelegate);
        }

        private readonly ILicenseChecker licenseChecker;
        private readonly ILogger<CommandHandler> logger;
        private readonly TerminalOptions options;
        private readonly IServiceProvider services;
    }
}