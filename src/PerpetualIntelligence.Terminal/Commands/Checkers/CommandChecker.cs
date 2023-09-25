/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Terminal.Configuration.Options;
using System.Collections.Generic;
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
        /// <param name="argumentChecker">The argument checker.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public CommandChecker(IOptionChecker optionChecker, IArgumentChecker argumentChecker, TerminalOptions options, ILogger<CommandChecker> logger)
        {
            this.optionChecker = optionChecker;
            this.argumentChecker = argumentChecker;
            this.terminalOptions = options;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<CommandCheckerResult> CheckAsync(CommandCheckerContext context)
        {
            await CheckArgumentsAsync(context);

            await CheckOptionsAsync(context);

            return new CommandCheckerResult();
        }

        private async Task CheckArgumentsAsync(CommandCheckerContext context)
        {
            // Cache commonly accessed properties
            var command = context.HandlerContext.ParsedCommand.Command;
            ArgumentDescriptors? argumentDescriptors = command.Descriptor.ArgumentDescriptors;

            // If the command itself does not support any arguments then there's nothing to check. Extractor will reject any unsupported attributes.
            if (argumentDescriptors == null)
            {
                return;
            }

            // Check the arguments against the descriptor constraints
            foreach (var arg in argumentDescriptors)
            {
                bool containsArg = command.TryGetArgument(arg.Id, out Argument? argument);
                var flags = arg.Flags;

                if (!containsArg)
                {
                    // Required argument is missing
                    if (flags.HasFlag(ArgumentFlags.Required))
                    {
                        throw new ErrorException(TerminalErrors.MissingArgument, "The required argument is missing. command={0} argument={1}", command.Id, arg.Id);
                    }

                    continue;  // Skip checking the other conditions if argument isn't present
                }

                // Check obsolete
                if (flags.HasFlag(ArgumentFlags.Obsolete) && !terminalOptions.Checker.AllowObsolete.GetValueOrDefault())
                {
                    throw new ErrorException(TerminalErrors.InvalidArgument, "The argument is obsolete. command={0} argument={1}", command.Id, arg.Id);
                }

                // Check disabled
                if (flags.HasFlag(ArgumentFlags.Disabled))
                {
                    throw new ErrorException(TerminalErrors.InvalidArgument, "The argument is disabled. command={0} argument={1}", command.Id, arg.Id);
                }

                // Check arg value
                await argumentChecker.CheckAsync(new ArgumentCheckerContext(argument!));
            }
        }

        private async Task CheckOptionsAsync(CommandCheckerContext context)
        {
            // Cache commonly accessed properties
            var command = context.HandlerContext.ParsedCommand.Command;
            OptionDescriptors? optionDescriptors = command.Descriptor.OptionDescriptors;

            // If the command itself does not support any options then there's nothing to check. Extractor will reject any unsupported attributes.
            if (optionDescriptors == null)
            {
                return;
            }

            // Check the options against the descriptor constraints
            foreach (KeyValuePair<string, OptionDescriptor> optKvp in optionDescriptors)
            {
                bool containsOpt = command.TryGetOption(optKvp.Key, out Option? opt);
                var flags = optKvp.Value.Flags;

                if (!containsOpt)
                {
                    // Required option is missing
                    if (flags.HasFlag(OptionFlags.Required))
                    {
                        throw new ErrorException(TerminalErrors.MissingOption, "The required option is missing. command={0} option={1}", command.Id, optKvp.Key);
                    }

                    continue;  // Skip checking the other conditions if option isn't present
                }

                // Check obsolete
                if (flags.HasFlag(OptionFlags.Obsolete) && !terminalOptions.Checker.AllowObsolete.GetValueOrDefault())
                {
                    throw new ErrorException(TerminalErrors.InvalidOption, "The option is obsolete. command={0} option={1}", command.Id, optKvp.Key);
                }

                // Check disabled
                if (flags.HasFlag(OptionFlags.Disabled))
                {
                    throw new ErrorException(TerminalErrors.InvalidOption, "The option is disabled. command={0} option={1}", command.Id, optKvp.Key);
                }

                // Check arg value
                await optionChecker.CheckAsync(new OptionCheckerContext(opt!));
            }
        }

        private readonly IOptionChecker optionChecker;
        private readonly IArgumentChecker argumentChecker;
        private readonly ILogger<CommandChecker> logger;
        private readonly TerminalOptions terminalOptions;
    }
}