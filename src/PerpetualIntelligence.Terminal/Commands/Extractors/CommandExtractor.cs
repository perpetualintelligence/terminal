/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Infrastructure;
using PerpetualIntelligence.Shared.Services;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    /// <summary>
    /// The default <see cref="ICommandExtractor"/>.
    /// </summary>
    /// <seealso cref="ExtractorOptions.Separator"/>
    public class CommandExtractor : ICommandExtractor
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="commandRouteParser">The command route parser.</param>
        /// <param name="commandStoreHandler">The command store handler.</param>
        /// <param name="optionExtractor">The option extractor.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="terminalOptions">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public CommandExtractor(
            ICommandRouteParser commandRouteParser,
            ICommandStoreHandler commandStoreHandler,
            IOptionExtractor optionExtractor,
            ITextHandler textHandler,
            TerminalOptions terminalOptions,
            ILogger<CommandExtractor> logger)
        {
            this.commandRouteParser = commandRouteParser ?? throw new ArgumentNullException(nameof(commandRouteParser));
            this.commandStoreHandler = commandStoreHandler ?? throw new ArgumentNullException(nameof(commandStoreHandler));
            this.optionExtractor = optionExtractor ?? throw new ArgumentNullException(nameof(optionExtractor));
            this.textHandler = textHandler;
            this.terminalOptions = terminalOptions ?? throw new ArgumentNullException(nameof(terminalOptions));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<CommandExtractorResult> ExtractAsync(CommandExtractorContext context)
        {
            // Find the command identify by prefix
            CommandDescriptor commandDescriptor = await MatchByPrefixAsync(context.Route.Command);

            // Extract the options. Options are optional for commands.
            Options? options = await ExtractOptionsOrThrowAsync(context, commandDescriptor);

            // This will be the new implementation
            Root root = await commandRouteParser.ParseAsync(context.Route);

            return new CommandExtractorResult(new ExtractedCommand(context.Route, new Command(commandDescriptor, options), root));
        }

        private async Task<Options?> ExtractOptionsOrThrowAsync(CommandExtractorContext context, CommandDescriptor commandDescriptor)
        {
            // Remove the prefix from the start so we can get the option string.
            string raw = context.Route.Command.Raw;
            string rawArgString = raw.TrimStart(commandDescriptor.Prefix, textHandler.Comparison);

            // Commands may not have options.
            if (!string.IsNullOrWhiteSpace(rawArgString))
            {
                // If options are passed make sure command supports options, exact options are checked later
                if (commandDescriptor.OptionDescriptors == null || commandDescriptor.OptionDescriptors.Count == 0)
                {
                    throw new ErrorException(TerminalErrors.UnsupportedOption, "The command does not support any options. command_name={0} command_id={1}", commandDescriptor.Name, commandDescriptor.Id);
                }

                // Make sure there is a separator between the command prefix and options
                if (!rawArgString.StartsWith(this.terminalOptions.Extractor.Separator, textHandler.Comparison))
                {
                    throw new ErrorException(TerminalErrors.InvalidCommand, "The command separator is missing. command_string={0}", raw);
                }
            }
            else
            {
                return null;
            }

            // TODO this should be a regex based parser
            // The argSplit string is used to split the options. This is to avoid splitting the option value
            // containing the separator. If space is the separator and - is the option prefix then the arg split
            // format is " -"
            // - E.g. -key1=val with space -key2=val2
            // - TODO: How to handle the arg string -key1=val with space and - in them -key2=value the current algorithm will
            // split the arg string into 3 parts but there are only 2 args. May be the string should be in quotes ""
            OptionStrings optionStrings = TerminalHelper.ExtractOptionStrings(rawArgString, terminalOptions, textHandler);

            List<Error> errors = new();
            Options options = new(textHandler);
            foreach (var argString in optionStrings)
            {
                // We capture all the option extraction errors
                TryResultOrError<OptionExtractorResult> tryResult = await InfraHelper.EnsureResultAsync(optionExtractor.ExtractAsync, new OptionExtractorContext(argString, commandDescriptor));
                if (tryResult.Error != null)
                {
                    errors.Add(tryResult.Error);
                }
                else
                {
                    // Protect for bad custom implementation.
                    if (tryResult.Result == null)
                    {
                        errors.Add(new Error(TerminalErrors.InvalidOption, "The option string did not return an error or extract the option. option_string={0}", argString.Raw));
                    }
                    else
                    {
                        // Avoid dictionary duplicate key and give meaningful error
                        if (options.Contains(tryResult.Result.Option))
                        {
                            errors.Add(new Error(TerminalErrors.DuplicateOption, "The option is already added to the command. option={0}", tryResult.Result.Option.Id));
                        }
                        else
                        {
                            options.Add(tryResult.Result.Option);
                        }
                    }
                }
            }

            if (errors.Count > 0)
            {
                throw new MultiErrorException(errors.ToArray());
            }

            return options;
        }

        /// <summary>
        /// Matches the command string and finds the <see cref="CommandDescriptor"/>.
        /// </summary>
        /// <param name="commandString">The command string to match.</param>
        /// <returns></returns>
        /// <exception cref="ErrorException">If command string did not match any command descriptor.</exception>
        private async Task<CommandDescriptor> MatchByPrefixAsync(CommandString commandString)
        {
            string prefix = commandString.Raw;

            // Find the prefix. Prefix is the entire string till first option or default option value. But the
            // default option is specified after the command prefix followed by command separator.
            // - E.g. pi auth login {default_arg_value}.
            int[] indices = new int[2];
            indices[0] = prefix.IndexOf(terminalOptions.Extractor.OptionPrefix, textHandler.Comparison);
            indices[1] = prefix.IndexOf(terminalOptions.Extractor.OptionAliasPrefix, textHandler.Comparison);
            int minIndex = indices.Where(x => x > 0).DefaultIfEmpty().Min();
            if (minIndex != 0)
            {
                prefix = prefix.Substring(0, minIndex);
            }

            // Make sure we trim the command separator. prefix from the previous step will most likely have a command separator
            // - E.g. pi auth login -key=value -> "pi auth login "
            //
            // At this point the prefix may also have default option value.
            // - E.g. pi auth login default_value
            prefix = prefix.TrimEnd(terminalOptions.Extractor.Separator, textHandler.Comparison);
            TryResultOrError<CommandDescriptor> result = await commandStoreHandler.TryMatchByPrefixAsync(prefix);
            if (result.Error != null)
            {
                throw new ErrorException(result.Error);
            }

            // Make sure we have the result to proceed. Protect bad custom implementations.
            if (result.Result == null)
            {
                throw new ErrorException(TerminalErrors.InvalidCommand, "The command string did not return an error or match the command prefix. command_string={0}", commandString);
            }

            // Make sure the command id is valid
            if (!Regex.IsMatch(result.Result.Id, terminalOptions.Extractor.CommandIdRegex))
            {
                throw new ErrorException(TerminalErrors.InvalidCommand, "The command identifier is not valid. command_id={0} regex={1}", result.Result.Id, terminalOptions.Extractor.CommandIdRegex);
            }

            return result.Result;
        }

        private readonly IOptionExtractor optionExtractor;
        private readonly ICommandRouteParser commandRouteParser;
        private readonly ICommandStoreHandler commandStoreHandler;
        private readonly ILogger<CommandExtractor> logger;
        private readonly TerminalOptions terminalOptions;
        private readonly ITextHandler textHandler;
    }
}