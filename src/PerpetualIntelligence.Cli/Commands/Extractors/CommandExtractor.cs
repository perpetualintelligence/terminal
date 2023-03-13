/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Providers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Cli.Stores;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Infrastructure;
using PerpetualIntelligence.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Extractors
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
        /// <param name="argumentExtractor">The argument extractor.</param>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="defaultArgumentProvider">The optional default argument provider.</param>
        /// <param name="defaultArgumentValueProvider">The optional argument default value provider.</param>
        public CommandExtractor(
            ICommandStoreHandler commandStoreHandler,
            IArgumentExtractor argumentExtractor,
            ITextHandler textHandler,
            CliOptions options,
            ILogger<CommandExtractor> logger,
            IDefaultArgumentProvider? defaultArgumentProvider = null,
            IDefaultArgumentValueProvider? defaultArgumentValueProvider = null)
        {
            this.commandStore = commandStoreHandler ?? throw new ArgumentNullException(nameof(commandStoreHandler));
            this.argumentExtractor = argumentExtractor ?? throw new ArgumentNullException(nameof(argumentExtractor));
            this.textHandler = textHandler;
            this.defaultArgumentValueProvider = defaultArgumentValueProvider;
            this.defaultArgumentProvider = defaultArgumentProvider;
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<CommandExtractorResult> ExtractAsync(CommandExtractorContext context)
        {
            // Find the command identify by prefix
            CommandDescriptor commandDescriptor = await MatchByPrefixAsync(context.CommandString);

            // Extract the arguments. Arguments are optional for commands.
            Options? arguments = await ExtractArgumentsOrThrowAsync(context, commandDescriptor);

            // Merge default argument.
            arguments = await MergeDefaultArgumentsOrThrowAsync(commandDescriptor, arguments);

            return new CommandExtractorResult(new Command(commandDescriptor, arguments), commandDescriptor);
        }

        private async Task<Options?> ExtractArgumentsOrThrowAsync(CommandExtractorContext context, CommandDescriptor commandDescriptor)
        {
            // Remove the prefix from the start so we can get the argument string.
            string raw = context.CommandString.Raw;
            string rawArgString = raw.TrimStart(commandDescriptor.Prefix, textHandler.Comparison);

            // Commands may not have arguments.
            if (!string.IsNullOrWhiteSpace(rawArgString))
            {
                // If arguments are passed make sure command supports arguments, exact arguments are checked later
                if (commandDescriptor.ArgumentDescriptors == null || commandDescriptor.ArgumentDescriptors.Count == 0)
                {
                    throw new ErrorException(Errors.UnsupportedArgument, "The command does not support any arguments. command_name={0} command_id={1}", commandDescriptor.Name, commandDescriptor.Id);
                }

                // Make sure there is a separator between the command prefix and arguments
                if (!rawArgString.StartsWith(options.Extractor.Separator, textHandler.Comparison))
                {
                    throw new ErrorException(Errors.InvalidCommand, "The command separator is missing. command_string={0}", raw);
                }
            }
            else
            {
                return null;
            }

            // Check if the command supports default argument. The default argument does not have standard argument
            // syntax For e.g. If 'pi format ruc' command has 'i' as a default argument then the command string 'pi
            // format ruc remove_underscore_and_capitalize' will be extracted as 'pi format ruc' and
            // remove_underscore_and_capitalize will be added as a value of argument 'i'.
            if (options.Extractor.DefaultArgument.GetValueOrDefault() && !string.IsNullOrWhiteSpace(commandDescriptor.DefaultArgument))
            {
                // Sanity check
                if (defaultArgumentProvider == null)
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The default argument provider is missing in the service collection. provider_type={0}", typeof(IDefaultArgumentValueProvider).FullName);
                }

                // Options and command supports the default argument, but is the default value provided by user ? If yes
                // then add the default attribute
                bool proccessDefaultArg = true;
                string argStringDef = rawArgString.TrimStart(options.Extractor.Separator, textHandler.Comparison);
                if (argStringDef.StartsWith(options.Extractor.ArgumentPrefix, textHandler.Comparison))
                {
                    // Default attribute value should be the first after command prefix User has explicitly passed an argument.
                    proccessDefaultArg = false;
                }

                if (proccessDefaultArg)
                {
                    // Get the default argument
                    DefaultArgumentProviderResult defaultArgumentProviderResult = await defaultArgumentProvider.ProvideAsync(new DefaultArgumentProviderContext(commandDescriptor));

                    // Convert the arg string to standard format and let the IArgumentExtractor extract the argument and
                    // its value. E.g. pi format ruc remove_underscore_and_capitalize -> pi format ruc -i=remove_underscore_and_capitalize
                    rawArgString = $"{options.Extractor.Separator}{options.Extractor.ArgumentPrefix}{defaultArgumentProviderResult.DefaultArgumentDescriptor.Id}{options.Extractor.ArgumentValueSeparator}{argStringDef}";
                }
            }

            // The argSplit string is used to split the arguments. This is to avoid splitting the argument value
            // containing the separator. If space is the separator and - is the argument prefix then the arg split
            // format is " -"
            // - E.g. -key1=val with space -key2=val2
            // - TODO: How to handle the arg string -key1=val with space and - in them -key2=value the current algorithm will
            // split the arg string into 3 parts but there are only 2 args. May be the string should be in quotes ""
            var argumentStrings = ExtractArgumentStrings(rawArgString);

            List<Error> errors = new();
            Options arguments = new(textHandler);
            foreach (var argString in argumentStrings)
            {
                // We capture all the argument extraction errors
                TryResultOrError<ArgumentExtractorResult> tryResult = await InfraHelper.EnsureResultAsync(argumentExtractor.ExtractAsync, new ArgumentExtractorContext(argString, commandDescriptor));
                if (tryResult.Error != null)
                {
                    errors.Add(tryResult.Error);
                }
                else
                {
                    // Protect for bad custom implementation.
                    if (tryResult.Result == null)
                    {
                        errors.Add(new Error(Errors.InvalidArgument, "The argument string did not return an error or extract the argument. argument_string={0}", argString.Raw));
                    }
                    else
                    {
                        // Avoid dictionary duplicate key and give meaningful error
                        if (arguments.Contains(tryResult.Result.Argument))
                        {
                            errors.Add(new Error(Errors.DuplicateArgument, "The argument is already added to the command. argument={0}", tryResult.Result.Argument.Id));
                        }
                        else
                        {
                            arguments.Add(tryResult.Result.Argument);
                        }
                    }
                }
            }

            if (errors.Count > 0)
            {
                throw new MultiErrorException(errors.ToArray());
            }

            return arguments;
        }

        private OptionStrings ExtractArgumentStrings(string raw)
        {
            string argSplit = string.Concat(options.Extractor.Separator, options.Extractor.ArgumentPrefix);
            string argAliasSplit = string.Concat(options.Extractor.Separator, options.Extractor.ArgumentAliasPrefix);

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

                    // If the min positive is the nextAliasPos then the next argument is identified by alias. If there
                    // is a conflict we give preference to argument id not alias.
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

                    // If the min positive is the nextAliasPos then the next argument is identified by alias. If there
                    // is a conflict we give preference to argument id not alias.
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

            // Find the prefix. Prefix is the entire string till first argument or default argument value. But the
            // default argument is specified after the command prefix followed by command separator.
            // - E.g. pi auth login {default_arg_value}.
            int[] indices = new int[2];
            indices[0] = prefix.IndexOf(options.Extractor.ArgumentPrefix, textHandler.Comparison);
            indices[1] = prefix.IndexOf(options.Extractor.ArgumentAliasPrefix, textHandler.Comparison);
            int minIndex = indices.Where(x => x > 0).DefaultIfEmpty().Min();
            if (minIndex != 0)
            {
                prefix = prefix.Substring(0, minIndex);
            }

            // Make sure we trim the command separator. prefix from the previous step will most likely have a command separator
            // - E.g. pi auth login -key=value -> "pi auth login "
            //
            // At this point the prefix may also have default argument value.
            // - E.g. pi auth login default_value
            prefix = prefix.TrimEnd(options.Extractor.Separator, textHandler.Comparison);
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
            if (!Regex.IsMatch(result.Result.Id, options.Extractor.CommandIdRegex))
            {
                throw new ErrorException(Errors.InvalidCommand, "The command identifier is not valid. command_id={0} regex={1}", result.Result.Id, options.Extractor.CommandIdRegex);
            }

            return result.Result;
        }

        /// <summary>
        /// Check for default arguments if enabled and merges then. Default values are added at the end if there is no
        /// explicit user input.
        /// </summary>
        /// <param name="commandDescriptor"></param>
        /// <param name="userArguments"></param>
        /// <returns></returns>
        /// <exception cref="ErrorException"></exception>
        /// <exception cref="MultiErrorException"></exception>
        private async Task<Options?> MergeDefaultArgumentsOrThrowAsync(CommandDescriptor commandDescriptor, Options? userArguments)
        {
            // If default argument value is disabled or the command itself does not support any arguments then ignore
            if (!options.Extractor.DefaultArgumentValue.GetValueOrDefault()
                || commandDescriptor.ArgumentDescriptors == null
                || commandDescriptor.ArgumentDescriptors.Count == 0)
            {
                return userArguments;
            }

            // Sanity check
            if (defaultArgumentValueProvider == null)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The argument default value provider is missing in the service collection. provider_type={0}", typeof(IDefaultArgumentValueProvider).FullName);
            }

            // Get default values. Make sure we take user inputs.
            Options? finalArgs = userArguments;
            DefaultArgumentValueProviderResult defaultResult = await defaultArgumentValueProvider.ProvideAsync(new DefaultArgumentValueProviderContext(commandDescriptor));
            if (defaultResult.DefaultValueArgumentDescriptors != null && defaultResult.DefaultValueArgumentDescriptors.Count > 0)
            {
                // arguments can be null here, if the command string did not specify any arguments
                if (finalArgs == null)
                {
                    finalArgs = new Options(textHandler);
                }

                List<Error> errors = new();
                foreach (OptionDescriptor argumentDescriptor in defaultResult.DefaultValueArgumentDescriptors)
                {
                    // Protect against bad implementation, catch all the errors
                    if (argumentDescriptor.DefaultValue == null)
                    {
                        errors.Add(new Error(Errors.InvalidArgument, "The argument does not have a default value. argument={0}", argumentDescriptor.Id));
                        continue;
                    }

                    // If user already specified the value then disregard the default value
                    if (userArguments == null || !userArguments.Contains(argumentDescriptor.Id))
                    {
                        finalArgs.Add(new Option(argumentDescriptor, argumentDescriptor.DefaultValue));
                    }
                }

                if (errors.Count > 0)
                {
                    throw new MultiErrorException(errors);
                }
            }

            return finalArgs;
        }

        private readonly IArgumentExtractor argumentExtractor;
        private readonly ICommandStoreHandler commandStore;
        private readonly IDefaultArgumentProvider? defaultArgumentProvider;
        private readonly IDefaultArgumentValueProvider? defaultArgumentValueProvider;
        private readonly ILogger<CommandExtractor> logger;
        private readonly CliOptions options;
        private readonly ITextHandler textHandler;
    }
}