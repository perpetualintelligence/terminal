/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;

using PerpetualIntelligence.Shared.Exceptions;
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
        /// <param name="argumentChecker">The option checker.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public CommandChecker(IOptionChecker argumentChecker, CliOptions options, ILogger<CommandChecker> logger)
        {
            this.argumentChecker = argumentChecker;
            this.options = options;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public virtual async Task<CommandCheckerResult> CheckAsync(CommandCheckerContext context)
        {
            // If the command itself do not support any options then there is nothing much to check. Extractor will
            // reject any unsupported attributes.
            if (context.CommandDescriptor.ArgumentDescriptors == null)
            {
                return new CommandCheckerResult();
            }

            // Check the options against the descriptor constraints
            // TODO: process multiple errors.
            foreach (var argDescriptor in context.CommandDescriptor.ArgumentDescriptors)
            {
                // Optimize (not all options are required)
                bool containsArg = context.Command.TryGetArgument(argDescriptor.Id, out Option? arg);
                if (!containsArg)
                {
                    // Required option is missing
                    if (argDescriptor.Required.GetValueOrDefault())
                    {
                        throw new ErrorException(Errors.MissingArgument, "The required option is missing. command_name={0} command_id={1} option={2}", context.Command.Name, context.Command.Id, argDescriptor.Id);
                    }
                }
                else
                {
                    // Check obsolete
                    if (argDescriptor.Obsolete.GetValueOrDefault() && !options.Checker.AllowObsoleteOption.GetValueOrDefault())
                    {
                        throw new ErrorException(Errors.InvalidArgument, "The option is obsolete. command_name={0} command_id={1} option={2}", context.Command.Name, context.Command.Id, argDescriptor.Id);
                    }

                    // Check disabled
                    if (argDescriptor.Disabled.GetValueOrDefault())
                    {
                        throw new ErrorException(Errors.InvalidArgument, "The option is disabled. command_name={0} command_id={1} option={2}", context.Command.Name, context.Command.Id, argDescriptor.Id);
                    }

                    // Check arg value
                    await argumentChecker.CheckAsync(new OptionCheckerContext(argDescriptor, arg!));
                }
            }

            return new CommandCheckerResult();
        }

        private readonly IOptionChecker argumentChecker;
        private readonly ILogger<CommandChecker> logger;
        private readonly CliOptions options;
    }
}
