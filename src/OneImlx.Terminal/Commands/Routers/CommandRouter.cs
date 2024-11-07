/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Events;
using OneImlx.Terminal.Licensing;
using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Routers
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
        /// <returns>The <see cref="CommandRouterResult"/> instance.</returns>
        public async Task<CommandRouterResult> RouteCommandAsync(CommandRouterContext context)
        {
            CommandRouterResult? result = null;
            ParsedCommand? extractedCommand = null;
            try
            {
                logger.LogDebug("Start command router. type={0} route={1}", this.GetType().Name, context.Request.Id);

                // Issue a before route event if configured
                if (asyncEventHandler != null)
                {
                    logger.LogDebug("Fire event. event={0} route={1}", nameof(asyncEventHandler.BeforeCommandRouteAsync), context.Request.Id);
                    await asyncEventHandler.BeforeCommandRouteAsync(context.Request);
                }

                // Ensure we have the license extracted before routing
                License? license = await licenseExtractor.GetLicenseAsync() ?? throw new TerminalException(TerminalErrors.InvalidLicense, "Failed to extract a valid license. Please configure the hosted service correctly.");

                // Parse the command
                CommandParserResult parserResult = await commandParser.ParseCommandAsync(new CommandParserContext(context.Request));
                extractedCommand = parserResult.ParsedCommand;

                // Delegate to handler
                CommandHandlerContext handlerContext = new(context, parserResult.ParsedCommand, license);
                var handlerResult = await commandHandler.HandleCommandAsync(handlerContext);
                result = new CommandRouterResult(handlerResult, context.Request);
            }
            finally
            {
                // Issue a after route event if configured
                if (asyncEventHandler != null)
                {
                    logger.LogDebug("Fire event. event={0} route={1}", nameof(asyncEventHandler.AfterCommandRouteAsync), context.Request.Id);
                    await asyncEventHandler.AfterCommandRouteAsync(context.Request, extractedCommand?.Command, result);
                }

                logger.LogDebug("End command router. route={0}", context.Request.Id);
            }

            return result;
        }

        private readonly ICommandParser commandParser;
        private readonly ICommandHandler commandHandler;
        private readonly ILogger<CommandRouter> logger;
        private readonly ITerminalEventHandler? asyncEventHandler;
        private readonly TerminalOptions terminalOptions;
        private readonly ILicenseExtractor licenseExtractor;
    }
}