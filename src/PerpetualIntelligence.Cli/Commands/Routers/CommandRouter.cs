/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Extractors;
using PerpetualIntelligence.Cli.Commands.Handlers;
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
        public CommandRouter(ILicenseExtractor licenseExtractor, ICommandExtractor commandExtractor, ICommandHandler commandHandler)
        {
            this.commandExtractor = commandExtractor ?? throw new ArgumentNullException(nameof(commandExtractor));
            this.licenseExtractor = licenseExtractor ?? throw new ArgumentNullException(nameof(licenseExtractor));
            this.commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
        }

        /// <summary>
        /// Routes the command request to the registered handler.
        /// </summary>
        /// <param name="context">The router context.</param>
        /// <returns>The <see cref="CommandRouterResult"/> instance.</returns>
        public async Task<CommandRouterResult> RouteAsync(CommandRouterContext context)
        {
            // Ensure we have the license extracted before routing
            Licensing.License? license = await licenseExtractor.GetLicenseAsync();
            if (license == null)
            {
                throw new ErrorException(Errors.InvalidLicense, "Failed to extract a valid license. Please configure the cli hosted service correctly.");
            }

            // Extract the command
            CommandExtractorResult extractorResult = await commandExtractor.ExtractAsync(new CommandExtractorContext(new CommandString(context.RawCommandString)));

            // Delegate to handler
            CommandHandlerContext handlerContext = new(context.Route, extractorResult.CommandDescriptor, extractorResult.Command, license);
            CommandHandlerResult result = await commandHandler.HandleAsync(handlerContext);
            return new CommandRouterResult(result, context.Route);
        }

        private readonly ICommandExtractor commandExtractor;
        private readonly ICommandHandler commandHandler;
        private readonly ILicenseExtractor licenseExtractor;
    }
}