/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Shared.Extensions;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The default command request router to route the request to its handler.
    /// </summary>
    public class CommandRequestRouter : ICommandRequestRouter
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="commands">The command identity store.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public CommandRequestRouter(ICommandIdentityStore commands, CliOptions options, ILogger<CommandRequestRouter> logger)
        {
            this.commands = commands;
            this.options = options;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<CommandRequestHandler?> FindHandlerAsync(CommandRequestContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Find the command
            var tryResult = await commands.TryFindMatchAsync(context.CommandString);
            if (tryResult.IsError)
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The request path did not match any command. path={0}", context.CommandString);
                return null;
            }
            else
            {
                return await GetRequestHandlerAsync(tryResult.Result, context);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RouteRequestAsync(CommandRequestContext context)
        {
            CommandRequestHandler? requestHandler = await FindHandlerAsync(context);
            if (requestHandler != null)
            {
                logger.LogInformation("Routing request. Handler={0} Path={1}", requestHandler.GetType().FullName, context.CommandString.ToString());

                CommandRequestResult result = await requestHandler.HandleRequestAsync(context);

                logger.LogTrace("Processing result. Type={0}", result.GetType().FullName);
                await result.ProcessResultAsync(context);

                return true;
            }
            else
            {
                return false;
            }
        }

        private Task<CommandRequestHandler?> GetRequestHandlerAsync(CommandIdentity command, CommandRequestContext context)
        {
            // FOMAC: yuck
            if (context.RequestServices.GetService<ICommandRequestHandler>() is CommandRequestHandler handler)
            {
                logger.LogDebug("Request handler matched. Command={0} ({1}), Handler={2}", command.Name, command.Id, command.RequestHandler.FullName);
                return Task.FromResult<CommandRequestHandler?>(handler); ;
            }

            logger.LogDebug("The command handler is missing. command_name={0} command_id={1}, handler={2}", command.Name, command.Id, command.RequestHandler.FullName);
            return Task.FromResult<CommandRequestHandler?>(null);
        }

        private readonly ICommandIdentityStore commands;
        private readonly ILogger<CommandRequestRouter> logger;
        private readonly CliOptions options;
    }
}
