/*
    Copyright 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands.Extractors;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Shared.Infrastructure;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Routers
{
    /// <summary>
    /// The command router.
    /// </summary>
    public class CommandRouter : ICommandRouter
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="extractor">The command extractor.</param>
        /// <param name="handler">The command handler.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public CommandRouter(ICommandExtractor extractor, ICommandHandler handler, CliOptions options, ILogger<CommandRouter> logger)
        {
            this.extrator = extractor ?? throw new ArgumentNullException(nameof(extractor));
            this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Routes the command request to the registered handler.
        /// </summary>
        /// <param name="context">The router context.</param>
        /// <returns>The <see cref="CommandRouterResult"/> instance.</returns>
        public virtual async Task<CommandRouterResult> RouteAsync(CommandRouterContext context)
        {
            // Extract the command
            CommandExtractorResult extractorResult = await extrator.ExtractAsync(new CommandExtractorContext(new CommandString(context.RawCommandString)));

            // Delegate to handler
            TryResultOrErrors<ICommandHandler> tryHandler = await TryFindHandlerAsync(context);
            CommandHandlerContext handlerContext = new(extractorResult.CommandDescriptor, extractorResult.Command);
            await tryHandler.Result!.HandleAsync(handlerContext);

            return new CommandRouterResult();
        }

        /// <inheritdoc/>
        public Task<TryResultOrErrors<ICommandHandler>> TryFindHandlerAsync(CommandRouterContext context)
        {
            // Dummy for design. We will always find the handler as its checked in constructor.
            return Task.FromResult(new TryResultOrErrors<ICommandHandler>(handler));
        }

        private readonly ICommandExtractor extrator;
        private readonly ICommandHandler handler;
        private readonly ILogger<CommandRouter> logger;
        private readonly CliOptions options;
    }
}
