/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
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
    /// The <c>cli</c> generic command router.
    /// </summary>
    public class CommandRouter : ICommandRouter
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="extractor">The command extractor.</param>
        /// <param name="handler">The command handler.</param>
        /// <param name="services">The services.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public CommandRouter(ICommandExtractor extractor, ICommandHandler handler, CliOptions options, ILogger<CommandRouter> logger)
        {
            this.extrator = extractor ?? throw new ArgumentNullException(nameof(extractor));
            this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public virtual async Task<CommandRouterResult> RouteAsync(CommandRouterContext context)
        {
            // Extract the command
            CommandExtractorResult tryResult = await extrator.ExtractAsync(new CommandExtractorContext(context.CommandString));
            if (tryResult.IsError)
            {
                return OneImlxResult.NewError<CommandRouterResult>(tryResult);
            }

            // Must extract command identity
            if (tryResult.CommandIdentity == null)
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The router failed to extract command identity. command_string={0} extractor={1}", context.CommandString, extrator.GetType().FullName);
                return OneImlxResult.NewError<CommandRouterResult>(Errors.ServerError, errorDesc, context.CommandString);
            }

            // Must extract command
            if (tryResult.Command == null)
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The router failed to extract command. command_string={0} extractor={1}", context.CommandString, extrator.GetType().FullName);
                return OneImlxResult.NewError<CommandRouterResult>(Errors.ServerError, errorDesc, context.CommandString);
            }

            // Delegate to handler
            OneImlxTryResult<ICommandHandler> tryHandler = await TryFindHandlerAsync(context);
            CommandHandlerContext handlerContext = new(tryResult.CommandIdentity, tryResult.Command);
            CommandHandlerResult handlerResult = await tryHandler.Result!.HandleAsync(handlerContext);
            if (handlerResult.IsError)
            {
                return OneImlxResult.NewError<CommandRouterResult>(handlerResult);
            }

            return new CommandRouterResult();
        }

        /// <inheritdoc/>
        public Task<OneImlxTryResult<ICommandHandler>> TryFindHandlerAsync(CommandRouterContext context)
        {
            // Dummy for design. We will always find the handler as its checked in constructor.
            return Task.FromResult(new OneImlxTryResult<ICommandHandler>(handler));
        }

        private readonly ICommandExtractor extrator;
        private readonly ICommandHandler handler;
        private readonly ILogger<CommandRouter> logger;
        private readonly CliOptions options;
    }
}
