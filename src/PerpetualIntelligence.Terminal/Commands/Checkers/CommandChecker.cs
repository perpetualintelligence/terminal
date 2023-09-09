﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Terminal.Configuration.Options;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Checkers
{
    /// <summary>
    /// The default command checker.
    /// </summary>
    public sealed class CommandChecker : ICommandChecker
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="optionChecker">The option checker.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public CommandChecker(IOptionChecker optionChecker, TerminalOptions options, ILogger<CommandChecker> logger)
        {
            this.optionChecker = optionChecker;
            this.options = options;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<CommandCheckerResult> CheckAsync(CommandCheckerContext context)
        {
            // If the command itself do not support any options then there is nothing much to check. Extractor will
            // reject any unsupported attributes.
            OptionDescriptors? optionDescriptors = context.HandlerContext.ExtractedCommand.Command.Descriptor.OptionDescriptors;
            if (optionDescriptors == null)
            {
                return new CommandCheckerResult();
            }

            // Check the options against the descriptor constraints
            // TODO: process multiple errors.
            foreach (var optDescriptor in optionDescriptors)
            {
                // Optimize (not all options are required)
                bool containsOpt = context.HandlerContext.ExtractedCommand.Command.TryGetOption(optDescriptor.Id, out Option? opt);
                if (!containsOpt)
                {
                    // Required option is missing
                    if (optDescriptor.Flags.HasFlag(OptionFlags.Required))
                    {
                        throw new ErrorException(TerminalErrors.MissingOption, "The required option is missing. command_name={0} command_id={1} option={2}", context.HandlerContext.ExtractedCommand.Command.Name, context.HandlerContext.ExtractedCommand.Command.Id, optDescriptor.Id);
                    }
                }
                else
                {
                    // Check obsolete
                    if (optDescriptor.Flags.HasFlag(OptionFlags.Obsolete) && !options.Checker.AllowObsoleteOption.GetValueOrDefault())
                    {
                        throw new ErrorException(TerminalErrors.InvalidOption, "The option is obsolete. command_name={0} command_id={1} option={2}", context.HandlerContext.ExtractedCommand.Command.Name, context.HandlerContext.ExtractedCommand.Command.Id, optDescriptor.Id);
                    }

                    // Check disabled
                    if (optDescriptor.Flags.HasFlag(OptionFlags.Disabled))
                    {
                        throw new ErrorException(TerminalErrors.InvalidOption, "The option is disabled. command_name={0} command_id={1} option={2}", context.HandlerContext.ExtractedCommand.Command.Name, context.HandlerContext.ExtractedCommand.Command.Id, optDescriptor.Id);
                    }

                    // Check arg value
                    await optionChecker.CheckAsync(new OptionCheckerContext(opt!));
                }
            }

            return new CommandCheckerResult();
        }

        private readonly IOptionChecker optionChecker;
        private readonly ILogger<CommandChecker> logger;
        private readonly TerminalOptions options;
    }
}