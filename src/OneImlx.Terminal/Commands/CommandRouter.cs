/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Events;
using OneImlx.Terminal.Licensing;
using OneImlx.Terminal.Shared;

namespace OneImlx.Terminal.Commands
{
    /// <summary>
    /// The default <see cref="ICommandRouter"/>.
    /// </summary>
    public sealed class CommandRouter : ICommandRouter
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="terminalOptions">The configuration options.</param>
        /// <param name="licenseExtractor">The license extractor.</param>
        /// <param name="commandParser">The command parser.</param>
        /// <param name="commandHandler">The command handler.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="asyncEventHandler">The event handler.</param>
        public CommandRouter(
            TerminalOptions terminalOptions,
            ILicenseExtractor licenseExtractor,
            ICommandParser commandParser,
            ICommandHandler commandHandler,
            ILogger<CommandRouter> logger,
            ITerminalEventHandler? asyncEventHandler = null)
        {
            this.commandParser = commandParser ?? throw new ArgumentNullException(nameof(commandParser));
            this.terminalOptions = terminalOptions ?? throw new ArgumentNullException(nameof(terminalOptions));
            this.licenseExtractor = licenseExtractor ?? throw new ArgumentNullException(nameof(licenseExtractor));
            this.commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
            this.logger = logger;
            this.asyncEventHandler = asyncEventHandler;
        }

        /// <summary>
        /// Routes the command request to the registered handler.
        /// </summary>
        /// <param name="context">The router context.</param>
        /// <returns>The <see cref="CommandResult"/> instance.</returns>
        public async Task<CommandResult> RouteCommandAsync(CommandContext context)
        {
            CommandResult? result = null;
            ParsedCommand? parsedCommand = null;
            try
            {
                logger.LogDebug("Start command router. type={0} request={1}", GetType().Name, context.Request.Id);

                // Issue a before request event if configured
                if (asyncEventHandler != null)
                {
                    logger.LogDebug("Fire event. event={0} request={1}", nameof(asyncEventHandler.BeforeCommandRouteAsync), context.Request.Id);
                    await asyncEventHandler.BeforeCommandRouteAsync(context.Request);
                }

                // Ensure we have the license extracted before routing
                License license = await licenseExtractor.GetLicenseAsync() ?? throw new TerminalException(TerminalErrors.InvalidLicense, "Failed to extract a valid license. Please configure the hosted service correctly.");
                context.License = license;

                // Parse the command
                await commandParser.ParseCommandAsync(context);
                parsedCommand = context.ParsedCommand;

                // Handle the command
                await commandHandler.HandleCommandAsync(context);

                // Ensure we have result.
                result = context.EnsureResult();
            }
            finally
            {
                // Issue a after request event if configured
                if (asyncEventHandler != null)
                {
                    logger.LogDebug("Fire event. event={0} request={1}", nameof(asyncEventHandler.AfterCommandRouteAsync), context.Request.Id);
                    await asyncEventHandler.AfterCommandRouteAsync(context.Request, parsedCommand?.Command, result);
                }

                logger.LogDebug("End command router. request={0}", context.Request.Id);
            }

            return result;
        }

        private readonly ITerminalEventHandler? asyncEventHandler;
        private readonly ICommandHandler commandHandler;
        private readonly ICommandParser commandParser;
        private readonly ILicenseExtractor licenseExtractor;
        private readonly ILogger<CommandRouter> logger;
        private readonly TerminalOptions terminalOptions;
    }
}
