/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Extractors;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Licensing;
using PerpetualIntelligence.Cli.Services;
using PerpetualIntelligence.Shared.Infrastructure;
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
        /// <param name="cliOptions">The cli options.</param>
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
            // Extract the license
            LicenseExtractorResult licExtractorResult = await licenseExtractor.ExtractAsync(new());

            // Extract the command
            CommandExtractorResult extractorResult = await commandExtractor.ExtractAsync(new CommandExtractorContext(new CommandString(context.RawCommandString)));

            // Delegate to handler
            TryResultOrErrors<ICommandHandler> tryHandler = await TryFindHandlerAsync(context);
            CommandHandlerContext handlerContext = new(extractorResult.CommandDescriptor, extractorResult.Command, licExtractorResult.License);
            await tryHandler.Result!.HandleAsync(handlerContext);

            return new CommandRouterResult();
        }

        /// <inheritdoc/>
        public Task<TryResultOrErrors<ICommandHandler>> TryFindHandlerAsync(CommandRouterContext context)
        {
            // Dummy for design. We will always find the handler as its checked in constructor.
            return Task.FromResult(new TryResultOrErrors<ICommandHandler>(commandHandler));
        }

        private readonly ICommandExtractor commandExtractor;
        private readonly ICommandHandler commandHandler;
        private readonly ILicenseExtractor licenseExtractor;
    }
}
