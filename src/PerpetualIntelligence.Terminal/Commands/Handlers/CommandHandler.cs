/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Terminal.Commands.Checkers;
using PerpetualIntelligence.Terminal.Commands.Providers;
using PerpetualIntelligence.Terminal.Commands.Routers;
using PerpetualIntelligence.Terminal.Commands.Runners;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Events;
using PerpetualIntelligence.Terminal.Licensing;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Handlers
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
            // Optional Event handler
            IAsyncEventHandler? asyncEventHandler = services.GetService<IAsyncEventHandler>();

            // Check the license
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(context.License));

            // Check and run the command
            Tuple<CommandCheckerResult, CommandRunnerResult> result = await CheckAndRunCommandInnerAsync(context, asyncEventHandler);

            // Return the processed result
            return new CommandHandlerResult(result.Item2, result.Item1);
        }

        private async Task<Tuple<CommandCheckerResult, CommandRunnerResult>> CheckAndRunCommandInnerAsync(CommandHandlerContext context, IAsyncEventHandler? asyncEventHandler)
        {
            // If we are executing a help command then we need to bypass all the checks.
            if (!options.Help.Disabled.GetValueOrDefault() &&
                (context.ParsedCommand.Command.TryGetOption(options.Help.OptionId, out _) ||
                 context.ParsedCommand.Command.TryGetOption(options.Help.OptionAlias, out _)
                ))
            {
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

        private async Task<CommandRunnerResult> RunCommandInnerAsync(CommandHandlerContext context, bool runHelp, IAsyncEventHandler? asyncEventHandler)
        {
            // Issue a before run event if configured
            if (asyncEventHandler != null)
            {
                await asyncEventHandler.BeforeCommandRunAsync(context.ParsedCommand.Command);
            }

            // Find the runner to run the command
            IDelegateCommandRunner commandRunner = await FindRunnerOrThrowAsync(context);
            CommandRunnerContext runnerContext = new(context);
            CommandRunnerResult runnerResult;

            // Run or Help
            if (runHelp)
            {
                IHelpProvider helpProvider = services.GetRequiredService<IHelpProvider>();
                runnerResult = await commandRunner.DelegateHelpAsync(runnerContext, helpProvider);
            }
            else
            {
                runnerResult = await commandRunner.DelegateRunAsync(runnerContext);
            }

            // Process the result.
            await runnerResult.ProcessAsync(runnerContext);

            // Issue a after run event if configured
            if (asyncEventHandler != null)
            {
                await asyncEventHandler.AfterCommandRunAsync(context.ParsedCommand.Command, runnerResult);
            }

            // Dispose the result.
            await runnerResult.DisposeAsync();

            return runnerResult;
        }

        private async Task<CommandCheckerResult> CheckCommandInnerAsync(CommandHandlerContext context, IAsyncEventHandler? asyncEventHandler)
        {
            // Issue a before check event if configured
            if (asyncEventHandler != null)
            {
                await asyncEventHandler.BeforeCommandCheckAsync(context.ParsedCommand.Command);
            }

            // Find the checker and check the command
            ICommandChecker commandChecker = await FindCheckerOrThrowAsync(context);
            var result = await commandChecker.CheckCommandAsync(new CommandCheckerContext(context));

            // Issue a after check event if configured
            if (asyncEventHandler != null)
            {
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

            return Task.FromResult(runnerDelegate);
        }

        private readonly ILicenseChecker licenseChecker;
        private readonly ILogger<CommandHandler> logger;
        private readonly TerminalOptions options;
        private readonly IServiceProvider services;
    }
}