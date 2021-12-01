/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Oidc;
using PerpetualIntelligence.Shared.Infrastructure;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The default command request router to route the request to its handler.
    /// </summary>
    public class CommandRouter : ICommandRouter
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="commands">The command identity store.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public CommandRouter(ICommandIdentityStore commands, CliOptions options, ILogger<CommandRouter> logger)
        {
            this.commands = commands;
            this.options = options;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<CommandResult> RouteAsync(CommandContext context)
        {
            var requestHandler = await TryFindHandlerAsync(context);
            if (!requestHandler.IsError)
            {
                logger.LogInformation("Routing request. Handler={0} Path={1}", requestHandler.GetType().FullName, context.CommandString.ToString());

                CommandResult result = await requestHandler.Result.HandleRequestAsync(context);

                logger.LogTrace("Processing result. Type={0}", result.GetType().FullName);
                await result.ProcessAsync(context);

                return result;
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<OneImlxTryResult<CommandHandler>> TryFindHandlerAsync(CommandContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Find the command
            var tryResult = await commands.TryFindMatchAsync(context.CommandString);
            if (tryResult.IsError)
            {
                OneImlxTryResult<CommandHandler> error = new OneImlxTryResult<CommandHandler>();
                error.SyncError(tryResult);
                return error;
            }
            else
            {
                CommandHandler? handler = await GetRequestHandlerAsync(tryResult.Result, context);
                if (handler != null)
                {
                    logger.LogDebug("Request handler matched. Command={0} ({1}), Handler={2}", tryResult.Result.Name, tryResult.Result.Id, tryResult.Result.RequestHandler.FullName);
                    return new OneImlxTryResult<CommandHandler>() { Result = handler };
                }
                else
                {
                    // FOMAC: fix logic and simplify
                    logger.LogDebug("The command handler is missing. command_name={0} command_id={1}, handler={2}", tryResult.Result.Name, tryResult.Result.Id, tryResult.Result.RequestHandler.FullName);
                    return OneImlxResult.NewError<OneImlxTryResult<CommandHandler>>(Errors.InvalidRequest, "");
                }
            }
        }

        private Task<CommandHandler?> GetRequestHandlerAsync(CommandIdentity command, CommandContext context)
        {
            // FOMAC: yuck
            if (context.RequestServices.GetService<ICommandRequestHandler>() is CommandHandler handler)
            {
                return Task.FromResult<CommandHandler?>(handler); ;
            }
            return Task.FromResult<CommandHandler?>(null);
        }

        private readonly ICommandIdentityStore commands;
        private readonly ILogger<CommandRouter> logger;
        private readonly CliOptions options;
    }
}
