/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands.Providers;
using PerpetualIntelligence.Cli.Commands.Stores;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Infrastructure;
using PerpetualIntelligence.Shared.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// The separator based command extractor.
    /// </summary>
    /// <seealso cref="ExtractorOptions.Separator"/>
    public class SeparatorCommandExtractor : ICommandExtractor
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="commandStore">The command descriptor store.</param>
        /// <param name="argumentExtractor">The argument extractor.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="defaultArgumentProvider">The optional default argument provider.</param>
        /// <param name="defaultArgumentValueProvider">The optional argument default value provider.</param>
        public SeparatorCommandExtractor(
            ICommandDescriptorStore commandStore,
            IArgumentExtractor argumentExtractor,
            CliOptions options,
            ILogger<SeparatorCommandExtractor> logger,
            IDefaultArgumentProvider? defaultArgumentProvider = null,
            IDefaultArgumentValueProvider? defaultArgumentValueProvider = null)
        {
            this.commandStore = commandStore ?? throw new ArgumentNullException(nameof(commandStore));
            this.argumentExtractor = argumentExtractor ?? throw new ArgumentNullException(nameof(argumentExtractor));
            this.defaultArgumentValueProvider = defaultArgumentValueProvider;
            this.defaultArgumentProvider = defaultArgumentProvider;
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<CommandExtractorResult> ExtractAsync(CommandExtractorContext context)
        {
            // Ensure that extractor options are compatible.
            EnsureOptionsCompatibility();

            // Find the command identify by prefix
            CommandDescriptor commandDescriptor = await MatchByPrefixAsync(context.CommandString);

            // Extract the arguments. Arguments are optional for commands.
            Arguments? arguments = await ExtractArgumentsOrThrowAsync(context, commandDescriptor);

            // Process default argument.
            arguments = await MergeDefaultArgumentsOrThrowAsync(commandDescriptor, arguments);

            // OK, return the extracted command object.
            Command command = new(commandDescriptor)
            {
                Arguments = arguments
            };

            return new CommandExtractorResult(command, commandDescriptor);
        }

        private void EnsureOptionsCompatibility()
        {
            // Separator can be whitespace
            if (options.Extractor.Separator == null)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The command separator is null or not configured.", options.Extractor.Separator);
            }

            // Command separator and argument separator cannot be same
            if (options.Extractor.Separator.Equals(options.Extractor.ArgumentSeparator, StringComparison.Ordinal))
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The command separator and argument separator cannot be same. separator={0}", options.Extractor.Separator);
            }

            // Command separator and argument prefix cannot be same
            if (options.Extractor.Separator.Equals(options.Extractor.ArgumentPrefix, StringComparison.Ordinal))
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The command separator and argument prefix cannot be same. separator={0}", options.Extractor.Separator);
            }

            // Argument separator cannot be null, empty or whitespace
            if (string.IsNullOrWhiteSpace(options.Extractor.ArgumentSeparator))
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The argument separator cannot be null or whitespace.", options.Extractor.ArgumentSeparator);
            }

            // Argument separator and argument prefix cannot be same
            if (options.Extractor.ArgumentSeparator.Equals(options.Extractor.ArgumentPrefix, StringComparison.Ordinal))
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The argument separator and argument prefix cannot be same. separator={0}", options.Extractor.ArgumentSeparator);
            }

            // Argument prefix cannot be null, empty or whitespace
            if (string.IsNullOrWhiteSpace(options.Extractor.ArgumentPrefix))
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The argument prefix cannot be null or whitespace.", options.Extractor.ArgumentPrefix);
            }

            // Command default argument provider is missing
            if (options.Extractor.DefaultArgument.GetValueOrDefault() && defaultArgumentProvider == null)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The command default argument provider is missing in the service collection. provider_type={0}", typeof(IDefaultArgumentProvider).FullName);
            }

            // Argument default value provider is missing
            if (options.Extractor.DefaultArgumentValue.GetValueOrDefault() && defaultArgumentValueProvider == null)
            {
                throw new ErrorException(Errors.InvalidConfiguration, "The argument default value provider is missing in the service collection. provider_type={0}", typeof(IDefaultArgumentValueProvider).FullName);
            }
        }

        private async Task<Arguments?> ExtractArgumentsOrThrowAsync(CommandExtractorContext context, CommandDescriptor commandDescriptor)
        {
            // Remove the prefix from the start so we can get the argument string.
            string raw = context.CommandString.Raw;
            string argString = raw.TrimStart(commandDescriptor.Prefix, StringComparison.Ordinal);

            // Commands may not have arguments.
            if (!string.IsNullOrWhiteSpace(argString))
            {
                // If arguments are passed make sure command supports arguments, exact arguments are checked later
                if (commandDescriptor.ArgumentDescriptors == null || commandDescriptor.ArgumentDescriptors.Count == 0)
                {
                    throw new ErrorException(Errors.UnsupportedArgument, "The command does not support any arguments. command_name={0} command_id={1}", commandDescriptor.Name, commandDescriptor.Id);
                }

                // Make sure there is a separator between the command prefix and arguments
                if (!argString.StartsWith(options.Extractor.Separator, StringComparison.Ordinal))
                {
                    throw new ErrorException(Errors.InvalidCommand, "The command separator is missing. command_string={0}", raw);
                }
            }
            else
            {
                return null;
            }

            // If we are here it
            // means arg starts with a separator


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

                // Get the default argument
                DefaultArgumentProviderResult defaultArgumentProviderResult = await defaultArgumentProvider.ProvideAsync(new DefaultArgumentProviderContext(commandDescriptor));

                // Convert the arg string to standard format and let the IArgumentExtractor extract the argument and its
                // value. E.g. pi format ruc remove_underscore_and_capitalize -> pi format ruc -i=remove_underscore_and_capitalize
                argString = $"{options.Extractor.Separator}{options.Extractor.ArgumentPrefix}{defaultArgumentProviderResult.DefaultArgumentDescriptor.Id}{options.Extractor.ArgumentSeparator}{argString.TrimStart(options.Extractor.Separator)}";
            }

            // The argument split string. This string is used to split the arguments. This is to avoid splitting the
            // argument value containing the separator. E.g. If space is the separator then the arg split format is ' -'
            // -Key1=val with space -Key2=val2
            // - TODO: How to handle the arg string -key1=val with space and - in them -key2=value the current algorithm will
            // split the arg string into 3 parts but there are only 2 args.
            string argSplit = string.Concat(options.Extractor.Separator, options.Extractor.ArgumentPrefix);

            Arguments arguments = new();
            string[] args = argString.Split(new string[] { argSplit }, StringSplitOptions.RemoveEmptyEntries);
            List<Error> errors = new();
            foreach (string arg in args)
            {
                string prefixArg = string.Concat(options.Extractor.ArgumentPrefix, arg);

                // We capture all the argument extraction errors
                TryResultOrError<ArgumentExtractorResult> tryResult = await Formatter.EnsureResultAsync(argumentExtractor.ExtractAsync, new ArgumentExtractorContext(prefixArg, commandDescriptor));
                if (tryResult.Error != null)
                {
                    errors.Add(tryResult.Error);
                }
                else
                {
                    // Protect for bad custom implementation.
                    if (tryResult.Result == null)
                    {
                        errors.Add(new Error(Errors.InvalidArgument, "The argument string did not return an error or extract the argument. argument_string={0}", prefixArg));
                    }
                    else
                    {
                        arguments.Add(tryResult.Result.Argument);
                    }
                }
            }

            if (errors.Count > 0)
            {
                throw new MultiErrorException(errors.ToArray());
            }

            return arguments;
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

            // Find the prefix. Prefix is the entire string till first argument or default argument value.
            // But the default argument is specified after the command prefix followed by command separator.
            // E.g. pi auth login {default_arg_value}. So it is difficult to determine the 
            // not use argument prefix, it uses the c so its not possible to determine
            int idx = prefix.IndexOf(options.Extractor.ArgumentPrefix, StringComparison.Ordinal);
            if (idx > 0)
            {
                prefix = prefix.Substring(0, idx);
            }

            // Make sure we trim the command separator. prefix from the previous step will most likely have a command
            // separator pi auth login -key=value
            prefix = prefix.TrimEnd(options.Extractor.Separator);
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
        private async Task<Arguments?> MergeDefaultArgumentsOrThrowAsync(CommandDescriptor commandDescriptor, Arguments? userArguments)
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
            Arguments? finalArgs = userArguments;
            DefaultArgumentValueProviderResult defaultResult = await defaultArgumentValueProvider.ProvideAsync(new DefaultArgumentValueProviderContext(commandDescriptor));
            if (defaultResult.DefaultValueArgumentDescriptors != null && defaultResult.DefaultValueArgumentDescriptors.Count > 0)
            {
                // arguments can be null here, if the command string did not specify any arguments
                if (finalArgs == null)
                {
                    finalArgs = new Arguments();
                }

                List<Error> errors = new();
                foreach (ArgumentDescriptor argumentDescriptor in defaultResult.DefaultValueArgumentDescriptors)
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
                        finalArgs.Add(new Argument(argumentDescriptor, argumentDescriptor.DefaultValue));
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
        private readonly ICommandDescriptorStore commandStore;
        private readonly IDefaultArgumentProvider? defaultArgumentProvider;
        private readonly IDefaultArgumentValueProvider? defaultArgumentValueProvider;
        private readonly ILogger<SeparatorCommandExtractor> logger;
        private readonly CliOptions options;
    }
}
