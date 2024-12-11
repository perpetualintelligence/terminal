/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneImlx.Terminal.Commands.Checkers;
using OneImlx.Terminal.Commands.Routers;
using OneImlx.Terminal.Commands.Runners;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Events;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Runtime;

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
            ICommandResolver commandResolver,
            ILicenseChecker licenseChecker,
            IOptions<TerminalOptions> options,
            ITerminalHelpProvider terminalHelpProvider,
            ILogger<CommandHandler> logger,
            ITerminalEventHandler? terminalEventHandler = null)
        {
            this.commandRuntime = commandResolver ?? throw new ArgumentNullException(nameof(commandResolver));
            this.licenseChecker = licenseChecker ?? throw new ArgumentNullException(nameof(licenseChecker));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.terminalHelpProvider = terminalHelpProvider ?? throw new ArgumentNullException(nameof(terminalHelpProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Optional, only is application wants events
            this.terminalEventHandler = terminalEventHandler;
        }

        /// <inheritdoc/>
        public async Task HandleCommandAsync(CommandRouterContext context)
        {
            logger.LogDebug("Handle request. request={0}", context.Request.Id);

            // Check the license
            await licenseChecker.CheckLicenseAsync(context.EnsureLicense());

            // Check and run the command
            Tuple<CommandCheckerResult, CommandRunnerResult> result = await CheckAndRunCommandInnerAsync(context);

            // Return the processed result
            context.Result = new CommandRouterResult(result.Item1, result.Item2);
        }

        private async Task<Tuple<CommandCheckerResult, CommandRunnerResult>> CheckAndRunCommandInnerAsync(CommandRouterContext context)
        {
            Command command = context.EnsureParsedCommand().Command;

            // If we are executing a help command then we need to bypass all the checks.
            if (!options.Value.Help.Disabled.GetValueOrDefault() &&
                (command.TryGetOption(options.Value.Help.OptionId, out Option? helpOption) ||
                 command.TryGetOption(options.Value.Help.OptionAlias, out helpOption)
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

        private async Task<CommandCheckerResult> CheckCommandInnerAsync(CommandRouterContext context)
        {
            Command command = context.EnsureParsedCommand().Command;

            // Issue a before check event if configured
            if (terminalEventHandler != null)
            {
                logger.LogDebug("Fire event. event={0} command={1}", nameof(terminalEventHandler.BeforeCommandCheckAsync), command.Id);
                await terminalEventHandler.BeforeCommandCheckAsync(command);
            }

            // Find the checker and check the command
            ICommandChecker commandChecker = commandRuntime.ResolveCommandChecker(command.Descriptor);
            var result = await commandChecker.CheckCommandAsync(context);

            // Issue a after check event if configured
            if (terminalEventHandler != null)
            {
                logger.LogDebug("Fire event. event={0} command={1}", nameof(terminalEventHandler.AfterCommandCheckAsync), command.Id);
                await terminalEventHandler.AfterCommandCheckAsync(command, result);
            }

            return result;
        }

        private async Task<CommandRunnerResult> RunCommandInnerAsync(CommandRouterContext context, bool runHelp)
        {
            Command command = context.EnsureParsedCommand().Command;

            // Issue a before run event if configured
            if (terminalEventHandler != null)
            {
                logger.LogDebug("Fire event. event={0} command={1}", nameof(terminalEventHandler.BeforeCommandRunAsync), command.Id);
                await terminalEventHandler.BeforeCommandRunAsync(command);
            }

            // Find the runner to run the command
            IDelegateCommandRunner commandRunner = commandRuntime.ResolveCommandRunner(command.Descriptor);
            CommandRunnerResult runnerResult;

            // Run or Help
            if (runHelp)
            {
                logger.LogDebug("Skip runner. Delegate to help provider. type={0}", terminalHelpProvider.GetType().Name);
                runnerResult = await commandRunner.DelegateHelpAsync(context, terminalHelpProvider, logger);
            }
            else
            {
                runnerResult = await commandRunner.DelegateRunAsync(context, logger);
            }

            // Issue a after run event if configured
            if (terminalEventHandler != null)
            {
                logger.LogDebug("Fire event. event={0} command={1}", nameof(terminalEventHandler.AfterCommandRunAsync), command.Id);
                await terminalEventHandler.AfterCommandRunAsync(command, runnerResult);
            }

            return runnerResult;
        }

        private readonly ICommandResolver commandRuntime;
        private readonly ILicenseChecker licenseChecker;
        private readonly ILogger<CommandHandler> logger;
        private readonly IOptions<TerminalOptions> options;
        private readonly ITerminalEventHandler? terminalEventHandler;
        private readonly ITerminalHelpProvider terminalHelpProvider;
    }
}
