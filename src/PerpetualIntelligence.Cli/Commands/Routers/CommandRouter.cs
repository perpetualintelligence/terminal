/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Cli.Commands.Extractors;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Events;
using PerpetualIntelligence.Cli.Licensing;
using PerpetualIntelligence.Shared.Exceptions;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Routers
{
    /// <summary>
    /// The default <see cref="ICommandRouter"/>.
    /// </summary>
    public sealed class CommandRouter : ICommandRouter
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="licenseExtractor">The license extractor.</param>
        /// <param name="commandExtractor">The command extractor.</param>
        /// <param name="commandHandler">The command handler.</param>
        /// <param name="asyncEventHandler">The event handler.</param>
        public CommandRouter(
            ILicenseExtractor licenseExtractor,
            ICommandExtractor commandExtractor,
            ICommandHandler commandHandler,
            IAsyncEventHandler? asyncEventHandler = null)
        {
            this.commandExtractor = commandExtractor ?? throw new ArgumentNullException(nameof(commandExtractor));
            this.licenseExtractor = licenseExtractor ?? throw new ArgumentNullException(nameof(licenseExtractor));
            this.commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
            this.asyncEventHandler = asyncEventHandler;
        }

        /// <summary>
        /// Routes the command request to the registered handler.
        /// </summary>
        /// <param name="context">The router context.</param>
        /// <returns>The <see cref="CommandRouterResult"/> instance.</returns>
        public async Task<CommandRouterResult> RouteAsync(CommandRouterContext context)
        {
            CommandRouterResult? result = null;
            Command? command = null;
            try
            {
                // Issue a before route event if configured
                if (asyncEventHandler != null)
                {
                    await asyncEventHandler.BeforeCommandRouteAsync(context.Route);
                }

                // Ensure we have the license extracted before routing
                Licensing.License? license = await licenseExtractor.GetLicenseAsync() ?? throw new ErrorException(Errors.InvalidLicense, "Failed to extract a valid license. Please configure the cli hosted service correctly.");

                // Extract the command
                CommandExtractorResult extractorResult = await commandExtractor.ExtractAsync(new CommandExtractorContext(context.Route));
                command = extractorResult.Command;

                // Delegate to handler
                CommandHandlerContext handlerContext = new(command, license);
                var handlerResult = await commandHandler.HandleAsync(handlerContext);
                result = new CommandRouterResult(handlerResult, context.Route);
            }
            finally
            {
                // Issue a after route event if configured
                if (asyncEventHandler != null)
                {
                    await asyncEventHandler.AfterCommandRouteAsync(context.Route, command, result);
                }
            }

            return result;
        }

        private readonly ICommandExtractor commandExtractor;
        private readonly ICommandHandler commandHandler;
        private readonly IAsyncEventHandler? asyncEventHandler;
        private readonly ILicenseExtractor licenseExtractor;
    }
}