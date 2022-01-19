/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands.Extractors;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Shared.Extensions;
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
            CommandExtractorResult tryResult = await extrator.ExtractAsync(new CommandExtractorContext(context.CommandString));
            if (tryResult.IsError)
            {
                return Result.NewError<CommandRouterResult>(tryResult);
            }

            // Must extract command identity
            if (tryResult.CommandIdentity == null)
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The router failed to extract command identity. command_string={0} extractor={1}", context.CommandString, extrator.GetType().FullName);
                return Result.NewError<CommandRouterResult>(Errors.ServerError, errorDesc, context.CommandString);
            }

            // Must extract command
            if (tryResult.Command == null)
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The router failed to extract command. command_string={0} extractor={1}", context.CommandString, extrator.GetType().FullName);
                return Result.NewError<CommandRouterResult>(Errors.ServerError, errorDesc, context.CommandString);
            }

            // Delegate to handler
            TryResult<ICommandHandler> tryHandler = await TryFindHandlerAsync(context);
            CommandHandlerContext handlerContext = new(tryResult.CommandIdentity, tryResult.Command);
            CommandHandlerResult handlerResult = await tryHandler.Result!.HandleAsync(handlerContext);
            if (handlerResult.IsError)
            {
                return Result.NewError<CommandRouterResult>(handlerResult);
            }

            return new CommandRouterResult();
        }

        /// <inheritdoc/>
        public Task<TryResult<ICommandHandler>> TryFindHandlerAsync(CommandRouterContext context)
        {
            // Dummy for design. We will always find the handler as its checked in constructor.
            return Task.FromResult(new TryResult<ICommandHandler>(handler));
        }

        private readonly ICommandExtractor extrator;
        private readonly ICommandHandler handler;
        private readonly ILogger<CommandRouter> logger;
        private readonly CliOptions options;
    }
}
