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
        public CommandHandler(
            ICommandRuntime commandRuntime,
            ILicenseChecker licenseChecker,
            TerminalOptions options,
            ITerminalHelpProvider terminalHelpProvider,
            ITerminalEventHandler? terminalEventHandler,
            ILogger<CommandHandler> logger)
        {
            this.commandRuntime = commandRuntime ?? throw new ArgumentNullException(nameof(commandRuntime));
            this.licenseChecker = licenseChecker ?? throw new ArgumentNullException(nameof(licenseChecker));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.terminalHelpProvider = terminalHelpProvider ?? throw new ArgumentNullException(nameof(terminalHelpProvider));
            this.terminalEventHandler = terminalEventHandler; // Optional
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initialize a news instance.
        /// </summary>
        public CommandHandler(
            ICommandRuntime commandRuntime,
            ILicenseChecker licenseChecker,
            TerminalOptions options,
            ITerminalHelpProvider terminalHelpProvider,
            ILogger<CommandHandler> logger): this (commandRuntime, licenseChecker, options, terminalHelpProvider, terminalEventHandler:null, logger)
        {
        }

        /// <inheritdoc/>
        public async Task<CommandHandlerResult> HandleCommandAsync(CommandHandlerContext context)
        {
            logger.LogDebug("Handle route. route={0}", context.RouterContext.Route.Id);

            // Check the license
            await licenseChecker.CheckLicenseAsync(new LicenseCheckerContext(context.License));

            // Check and run the command
            Tuple<CommandCheckerResult, CommandRunnerResult> result = await CheckAndRunCommandInnerAsync(context);

            // Return the processed result
            return new CommandHandlerResult(result.Item2, result.Item1);
        }

        private async Task<Tuple<CommandCheckerResult, CommandRunnerResult>> CheckAndRunCommandInnerAsync(CommandHandlerContext context)
        {
            // If we are executing a help command then we need to bypass all the checks.
            if (!options.Help.Disabled.GetValueOrDefault() &&
                (context.ParsedCommand.Command.TryGetOption(options.Help.OptionId, out Option? helpOption) ||
                 context.ParsedCommand.Command.TryGetOption(options.Help.OptionAlias, out helpOption)
                ))
            {
                logger.LogDebug("Found help option. option={0}", helpOption != null ? helpOption.Id : "?");
                CommandRunnerResult runnerResult = await RunCommandInnerAsync(context, runHelp: true);
                return new Tuple<CommandCheckerResult, CommandRunnerResult>(new CommandCheckerResult(), runnerResult);
            }
            else
            {
                CommandCheckerResult checkerResult = await CheckCommandInnerAsync(context);
                CommandRunnerResult runnerResult = await RunCommandInnerAsync(context, runHelp: false);
                return new Tuple<CommandCheckerResult, CommandRunnerResult>(checkerResult, runnerResult);
            }
        }

        private async Task<CommandRunnerResult> RunCommandInnerAsync(CommandHandlerContext context, bool runHelp)
        {
            // Issue a before run event if configured
            if (terminalEventHandler != null)
            {
                logger.LogDebug("Fire event. event={0} command={1}", nameof(terminalEventHandler.BeforeCommandRunAsync), context.ParsedCommand.Command.Id);
                await terminalEventHandler.BeforeCommandRunAsync(context.ParsedCommand.Command);
            }

            // Find the runner to run the command
            IDelegateCommandRunner commandRunner = commandRuntime.ResolveCommandRunner(context.ParsedCommand.Command.Descriptor);
            CommandRunnerContext runnerContext = new(context);
            CommandRunnerResult runnerResult;

            // Run or Help
            if (runHelp)
            {
                logger.LogDebug("Skip runner. Delegate to help provider. type={0}", terminalHelpProvider.GetType().Name);
                runnerResult = await commandRunner.DelegateHelpAsync(runnerContext, terminalHelpProvider, logger);
            }
            else
            {
                runnerResult = await commandRunner.DelegateRunAsync(runnerContext, logger);
            }

            // Process the result.
            await runnerResult.ProcessAsync(runnerContext, logger);

            // Issue a after run event if configured
            if (terminalEventHandler != null)
            {
                logger.LogDebug("Fire event. event={0} command={1}", nameof(terminalEventHandler.AfterCommandRunAsync), context.ParsedCommand.Command.Id);
                await terminalEventHandler.AfterCommandRunAsync(context.ParsedCommand.Command, runnerResult);
            }

            // Dispose the result.
            await runnerResult.DisposeAsync();

            return runnerResult;
        }

        private async Task<CommandCheckerResult> CheckCommandInnerAsync(CommandHandlerContext context)
        {
            // Issue a before check event if configured
            if (terminalEventHandler != null)
            {
                logger.LogDebug("Fire event. event={0} command={1}", nameof(terminalEventHandler.BeforeCommandCheckAsync), context.ParsedCommand.Command.Id);
                await terminalEventHandler.BeforeCommandCheckAsync(context.ParsedCommand.Command);
            }

            // Find the checker and check the command
            ICommandChecker commandChecker = commandRuntime.ResolveCommandChecker(context.ParsedCommand.Command.Descriptor);
            var result = await commandChecker.CheckCommandAsync(new CommandCheckerContext(context));

            // Issue a after check event if configured
            if (terminalEventHandler != null)
            {
                logger.LogDebug("Fire event. event={0} command={1}", nameof(terminalEventHandler.AfterCommandCheckAsync), context.ParsedCommand.Command.Id);
                await terminalEventHandler.AfterCommandCheckAsync(context.ParsedCommand.Command, result);
            }

            return result;
        }

        private readonly ILicenseChecker licenseChecker;
        private readonly ILogger<CommandHandler> logger;
        private readonly TerminalOptions options;
        private readonly ITerminalHelpProvider terminalHelpProvider;
        private readonly ITerminalEventHandler? terminalEventHandler;
        private readonly ICommandRuntime commandRuntime;
    }
}