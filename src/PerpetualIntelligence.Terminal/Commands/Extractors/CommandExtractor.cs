/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Commands.Providers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Terminal.Stores;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Infrastructure;
using PerpetualIntelligence.Shared.Services;
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
        /// <param name="commandStoreHandler">The command store handler.</param>
        /// <param name="optionExtractor">The option extractor.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="terminalOptions">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="defaultOptionProvider">The optional default option provider.</param>
        /// <param name="defaultOptionValueProvider">The optional option default value provider.</param>
        public CommandExtractor(
            ICommandStoreHandler commandStoreHandler,
            IOptionExtractor optionExtractor,
            ITextHandler textHandler,
            TerminalOptions terminalOptions,
            ILogger<CommandExtractor> logger,
            IDefaultOptionProvider? defaultOptionProvider = null,
            IDefaultOptionValueProvider? defaultOptionValueProvider = null)
        {
            this.commandStore = commandStoreHandler ?? throw new ArgumentNullException(nameof(commandStoreHandler));
            this.optionExtractor = optionExtractor ?? throw new ArgumentNullException(nameof(optionExtractor));
            this.textHandler = textHandler;
            this.defaultOptionValueProvider = defaultOptionValueProvider;
            this.defaultOptionProvider = defaultOptionProvider;
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

            // Merge default option.
            options = await MergeDefaultOptionsOrThrowAsync(commandDescriptor, options);

            return new CommandExtractorResult(new Command(context.Route, commandDescriptor, options));
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
                    throw new ErrorException(Errors.UnsupportedOption, "The command does not support any options. command_name={0} command_id={1}", commandDescriptor.Name, commandDescriptor.Id);
                }

                // Make sure there is a separator between the command prefix and options
                if (!rawArgString.StartsWith(this.terminalOptions.Extractor.Separator, textHandler.Comparison))
                {
                    throw new ErrorException(Errors.InvalidCommand, "The command separator is missing. command_string={0}", raw);
                }
            }
            else
            {
                return null;
            }

            // Check if the command supports default option. The default option does not have standard option
            // syntax For e.g. If 'pi format ruc' command has 'i' as a default option then the command string 'pi
            // format ruc remove_underscore_and_capitalize' will be extracted as 'pi format ruc' and
            // remove_underscore_and_capitalize will be added as a value of option 'i'.
            if (this.terminalOptions.Extractor.DefaultOption.GetValueOrDefault() && !string.IsNullOrWhiteSpace(commandDescriptor.DefaultOption))
            {
                // Sanity check
                if (defaultOptionProvider == null)
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The default option provider is missing in the service collection. provider_type={0}", typeof(IDefaultOptionValueProvider).Name);
                }

                // Options and command supports the default option, but is the default value provided by user ? If yes
                // then add the default attribute
                bool proccessDefaultArg = true;
                string argStringDef = rawArgString.TrimStart(this.terminalOptions.Extractor.Separator, textHandler.Comparison);
                if (argStringDef.StartsWith(this.terminalOptions.Extractor.OptionPrefix, textHandler.Comparison))
                {
                    // Default attribute value should be the first after command prefix User has explicitly passed an option.
                    proccessDefaultArg = false;
                }

                if (proccessDefaultArg)
                {
                    // Get the default option
                    DefaultOptionProviderResult defaultOptionProviderResult = await defaultOptionProvider.ProvideAsync(new DefaultOptionProviderContext(commandDescriptor));

                    // Convert the arg string to standard format and let the IArgumentExtractor extract the option and
                    // its value. E.g. pi format ruc remove_underscore_and_capitalize -> pi format ruc -i=remove_underscore_and_capitalize
                    rawArgString = $"{this.terminalOptions.Extractor.Separator}{this.terminalOptions.Extractor.OptionPrefix}{defaultOptionProviderResult.DefaultOptionDescriptor.Id}{this.terminalOptions.Extractor.OptionValueSeparator}{argStringDef}";
                }
            }

            // The argSplit string is used to split the options. This is to avoid splitting the option value
            // containing the separator. If space is the separator and - is the option prefix then the arg split
            // format is " -"
            // - E.g. -key1=val with space -key2=val2
            // - TODO: How to handle the arg string -key1=val with space and - in them -key2=value the current algorithm will
            // split the arg string into 3 parts but there are only 2 args. May be the string should be in quotes ""
            var optionStrings = ExtractOptionStrings(rawArgString);

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
                        errors.Add(new Error(Errors.InvalidOption, "The option string did not return an error or extract the option. option_string={0}", argString.Raw));
                    }
                    else
                    {
                        // Avoid dictionary duplicate key and give meaningful error
                        if (options.Contains(tryResult.Result.Option))
                        {
                            errors.Add(new Error(Errors.DuplicateOption, "The option is already added to the command. option={0}", tryResult.Result.Option.Id));
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

        private OptionStrings ExtractOptionStrings(string raw)
        {
            string argSplit = string.Concat(terminalOptions.Extractor.Separator, terminalOptions.Extractor.OptionPrefix);
            string argAliasSplit = string.Concat(terminalOptions.Extractor.Separator, terminalOptions.Extractor.OptionAliasPrefix);

            // First pass
            int currentPos = 0;
            bool currentIsAlias = false;
            int nextIdx = 0;
            OptionStrings locations = new();
            while (true)
            {
                // No more matches so break. When the currentPos reaches the end then we have traversed the entire argString.
                if (currentPos >= raw.Length)
                {
                    break;
                }

                // Initialize the iterators. For each iteration we assume that arg sub string is identifier by an
                // identifier and not alias so the default value for isAlias is false.
                int nextArgPos;
                int nextAliasPos;
                bool nextIsAlias = false;

                // First pass
                if (currentPos == 0)
                {
                    // First time we have to make multiple passes to determine whether the first is arg prefix or alias prefix
                    nextArgPos = raw.IndexOf(argSplit, currentPos, textHandler.Comparison);
                    nextAliasPos = raw.IndexOf(argAliasSplit, currentPos, textHandler.Comparison);

                    // Since this is the first iteration the minimum can be 0
                    nextIdx = InfraHelper.MinPositiveOrZero(nextArgPos, nextAliasPos);

                    // If the min positive is the nextAliasPos then the next option is identified by alias. If there
                    // is a conflict we give preference to option id not alias.
                    currentIsAlias = nextIdx != nextArgPos;
                }

                // Get next positions
                nextArgPos = raw.IndexOf(argSplit, nextIdx + 1, textHandler.Comparison);
                nextAliasPos = raw.IndexOf(argAliasSplit, nextIdx + 1, textHandler.Comparison);

                // We reached the end of positions for both, take the remaining string. This condition also help in
                // breaking the loop since we have traversed the argString now !
                if (nextArgPos < 0 && nextAliasPos < 0)
                {
                    nextIdx = raw.Length;
                }
                else
                {
                    // Min positive
                    // TODO: Improve performance
                    nextIdx = InfraHelper.MinPositiveOrZero(nextArgPos, nextAliasPos);

                    // If the min positive is the nextAliasPos then the next option is identified by alias. If there
                    // is a conflict we give preference to option id not alias.
                    nextIsAlias = nextIdx != nextArgPos;
                }

                // Get the arg substring and record its position and alias
                // NOTE: This is the current pos and current alias not the next.
                string kvp = raw.Substring(currentPos, nextIdx - currentPos);
                locations.Add(new OptionString(kvp, currentIsAlias, currentPos));

                // Move next
                currentPos = nextIdx;
                currentIsAlias = nextIsAlias;
            }

            return locations;
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
            TryResultOrError<CommandDescriptor> result = await commandStore.TryMatchByPrefixAsync(prefix);
            if (result.Error != null)
            {
                throw new ErrorException(result.Error);
            }

            // Make sure we have the result to proceed. Protect bad custom implementations.
            if (result.Result == null)
            {
                throw new ErrorException(Errors.InvalidCommand, "The command string did not return an error or match the command prefix. command_string={0}", commandString);
            }

            // Make sure the command id is valid
            if (!Regex.IsMatch(result.Result.Id, terminalOptions.Extractor.CommandIdRegex))
            {
                throw new ErrorException(Errors.InvalidCommand, "The command identifier is not valid. command_id={0} regex={1}", result.Result.Id, terminalOptions.Extractor.CommandIdRegex);
            }

            return result.Result;
        }

        /// <summary>
        /// Check for default options if enabled and merges then. Default values are added at the end if there is no
        /// explicit user input.
        /// </summary>
        /// <param name="commandDescriptor"></param>
        /// <param name="userOptions"></param>
        /// <returns></returns>
        /// <exception cref="ErrorException"></exception>
        /// <exception cref="MultiErrorException"></exception>
        private async Task<Options?> MergeDefaultOptionsOrThrowAsync(CommandDescriptor commandDescriptor, Options? userOptions)
        {
            // If default option value is disabled or the command itself does not support any options then ignore
            if (!terminalOptions.Extractor.DefaultOptionValue.GetValueOrDefault()
                || commandDescriptor.OptionDescriptors == null
                || commandDescriptor.OptionDescriptors.Count == 0)
            {
                return userOptions;
            }

            // Sanity check
            if (defaultOptionValueProvider == null)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The option default value provider is missing in the service collection. provider_type={0}", typeof(IDefaultOptionValueProvider).Name);
            }

            // Get default values. Make sure we take user inputs.
            Options? finalArgs = userOptions;
            DefaultOptionValueProviderResult defaultResult = await defaultOptionValueProvider.ProvideAsync(new DefaultOptionValueProviderContext(commandDescriptor));
            if (defaultResult.DefaultValueOptionDescriptors != null && defaultResult.DefaultValueOptionDescriptors.Count > 0)
            {
                // options can be null here, if the command string did not specify any options
                finalArgs ??= new Options(textHandler);

                List<Error> errors = new();
                foreach (OptionDescriptor optionDescriptor in defaultResult.DefaultValueOptionDescriptors)
                {
                    // Protect against bad implementation, catch all the errors
                    if (optionDescriptor.DefaultValue == null)
                    {
                        errors.Add(new Error(Errors.InvalidOption, "The option does not have a default value. option={0}", optionDescriptor.Id));
                        continue;
                    }

                    // If user already specified the value then disregard the default value
                    if (userOptions == null || !userOptions.Contains(optionDescriptor.Id))
                    {
                        finalArgs.Add(new Option(optionDescriptor, optionDescriptor.DefaultValue));
                    }
                }

                if (errors.Count > 0)
                {
                    throw new MultiErrorException(errors);
                }
            }

            return finalArgs;
        }

        private readonly IOptionExtractor optionExtractor;
        private readonly ICommandStoreHandler commandStore;
        private readonly IDefaultOptionProvider? defaultOptionProvider;
        private readonly IDefaultOptionValueProvider? defaultOptionValueProvider;
        private readonly ILogger<CommandExtractor> logger;
        private readonly TerminalOptions terminalOptions;
        private readonly ITextHandler textHandler;
    }
}