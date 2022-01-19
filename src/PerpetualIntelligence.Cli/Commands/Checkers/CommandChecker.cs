﻿/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    /// <summary>
    /// The command checker.
    /// </summary>
    public class CommandChecker : ICommandChecker
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="dataTypeChecker">The argument data-type checker.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public CommandChecker(IArgumentChecker dataTypeChecker, CliOptions options, ILogger<CommandChecker> logger)
        {
            this.valueChecker = dataTypeChecker;
            this.options = options;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public virtual async Task<CommandCheckerResult> CheckAsync(CommandCheckerContext context)
        {
            // FOMAC: The default extractor will filter out unsupported arguments. So we don't check it here.

            // The user may not pass any arguments.
            Dictionary<string, Argument> args = new();
            if (context.Command.Arguments != null)
            {
                args = context.Command.Arguments.ToNameArgumentDisctionary();
            }

            // If the command itself do not support any arguments then there is nothing much to check. Extractor will
            // reject any unsupported attributes.
            if (context.CommandIdentity.ArgumentIdentities == null)
            {
                return new CommandCheckerResult();
            }

            // Check the augments against the identity constraints
            // TODO: process multiple errors.
            foreach (var argIdentity in context.CommandIdentity.ArgumentIdentities)
            {
                // Optimize (not all arguments are required)
                bool containsArg = args.TryGetValue(argIdentity.Id, out Argument? arg);
                if (!containsArg)
                {
                    // Required argument is missing
                    if (argIdentity.IsRequired)
                    {
                        string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The required argument is missing. command_name={0} command_id={1} argument={2}", context.Command.Name, context.Command.Id, argIdentity.Id);
                        return Result.NewError<CommandCheckerResult>(Errors.MissingArgument, errorDesc);
                    }
                }
                else
                {
                    // Check obsolete
                    if (argIdentity.Obsolete.GetValueOrDefault() && !options.Checker.AllowObsoleteArgument.GetValueOrDefault())
                    {
                        string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument is obsolete. command_name={0} command_id={1} argument={2}", context.Command.Name, context.Command.Id, argIdentity.Id);
                        return Result.NewError<CommandCheckerResult>(Errors.InvalidArgument, errorDesc);
                    }

                    // Check disabled
                    if (argIdentity.Disabled.GetValueOrDefault())
                    {
                        string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument is disabled. command_name={0} command_id={1} argument={2}", context.Command.Name, context.Command.Id, argIdentity.Id);
                        return Result.NewError<CommandCheckerResult>(Errors.InvalidArgument, errorDesc);
                    }

                    // Check arg value
                    var dataTypeResult = await valueChecker.CheckAsync(new ArgumentCheckerContext(argIdentity, arg!));
                    if (dataTypeResult.IsError)
                    {
                        return Result.NewError<CommandCheckerResult>(dataTypeResult);
                    }
                }
            }

            return new CommandCheckerResult();
        }

        private readonly ILogger<CommandChecker> logger;
        private readonly CliOptions options;
        private readonly IArgumentChecker valueChecker;
    }
}
