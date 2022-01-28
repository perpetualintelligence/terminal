/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Shared.Exceptions;
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

            // If the command itself do not support any arguments then there is nothing much to check. Extractor will
            // reject any unsupported attributes.
            if (context.CommandDescriptor.ArgumentDescriptors == null)
            {
                return new CommandCheckerResult();
            }

            // Check the arguments against the descriptor constraints
            // TODO: process multiple errors.
            foreach (var argDescriptor in context.CommandDescriptor.ArgumentDescriptors)
            {
                // Optimize (not all arguments are required)
                bool containsArg = context.Command.TryGetArgument(argDescriptor.Id, out Argument? arg);
                if (!containsArg)
                {
                    // Required argument is missing
                    if (argDescriptor.IsRequired)
                    {
                        throw new ErrorException(Errors.MissingArgument, "The required argument is missing. command_name={0} command_id={1} argument={2}", context.Command.Name, context.Command.Id, argDescriptor.Id);
                    }
                }
                else
                {
                    // Check obsolete
                    if (argDescriptor.Obsolete.GetValueOrDefault() && !options.Checker.AllowObsoleteArgument.GetValueOrDefault())
                    {
                        throw new ErrorException(Errors.InvalidArgument, "The argument is obsolete. command_name={0} command_id={1} argument={2}", context.Command.Name, context.Command.Id, argDescriptor.Id);
                    }

                    // Check disabled
                    if (argDescriptor.Disabled.GetValueOrDefault())
                    {
                        throw new ErrorException(Errors.InvalidArgument, "The argument is disabled. command_name={0} command_id={1} argument={2}", context.Command.Name, context.Command.Id, argDescriptor.Id);
                    }

                    // Check arg value
                    await valueChecker.CheckAsync(new ArgumentCheckerContext(argDescriptor, arg!));
                }
            }

            return new CommandCheckerResult();
        }

        private readonly ILogger<CommandChecker> logger;
        private readonly CliOptions options;
        private readonly IArgumentChecker valueChecker;
    }
}
