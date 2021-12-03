/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Oidc;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    /// <summary>
    /// The <c>oneimlx</c> generic command checker.
    /// </summary>
    public class CommandChecker : ICommandChecker
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public CommandChecker(CliOptions options, ILogger<CommandChecker> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public virtual Task<CommandCheckerResult> CheckAsync(CommandCheckerContext context)
        {
            // The user may not pass any args so they can be null.
            Dictionary<string, object> args = new();
            if (context.Command.Arguments != null)
            {
                args = context.Command.Arguments.ToDictionary();
            }

            // If the command does not support any arguments then there is nothing much to check.
            if (context.CommandIdentity.Arguments == null)
            {
                // User pass unexpected arguments
                if (args.Count != 0)
                {
                    string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The command does not support any arguments. command_name={0} command_id={1} arguments={2}", context.Command.Name, context.Command.Id, args.Keys.JoinSpace());
                    return Task.FromResult(OneImlxResult.NewError<CommandCheckerResult>(Errors.InvalidRequest, errorDesc));
                }

                return Task.FromResult(new CommandCheckerResult());
            }

            // Make sure the command has the supported args based on command definition
            IEnumerable<string> invalidArgs = args.Keys.Except(context.CommandIdentity.Arguments.Select(e => e.Name));
            if (invalidArgs.Any())
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The arguments are not valid. command_name={0} command_id={1} arguments={2}", context.Command.Name, context.Command.Id, invalidArgs.JoinSpace());
                return Task.FromResult(OneImlxResult.NewError<CommandCheckerResult>(Errors.InvalidRequest, errorDesc));
            }

            // Check for required attributes
            IEnumerable<string> requiredArgs = context.CommandIdentity.Arguments.Where(a => a.Required).Select(e => e.Name);
            var missingArgs = requiredArgs.Except(args.Keys.AsEnumerable());
            if (missingArgs.Any())
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The arguments are missing. command_name={0} command_id={1} arguments={2}", context.Command.Name, context.Command.Id, missingArgs.JoinSpace());
                return Task.FromResult(OneImlxResult.NewError<CommandCheckerResult>(Errors.InvalidRequest, errorDesc));
            }

            // TODO check arg data type

            return Task.FromResult(new CommandCheckerResult());
        }

        private readonly ILogger<CommandChecker> logger;
        private readonly CliOptions options;
    }
}
