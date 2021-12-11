/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands.Stores;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Infrastructure;
using System;
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
        /// <param name="commandLookup">The command lookup.</param>
        /// <param name="argumentExtractor">The argument extractor.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public SeparatorCommandExtractor(ICommandIdentityStore commandLookup, IArgumentExtractor argumentExtractor, CliOptions options, ILogger<SeparatorCommandExtractor> logger)
        {
            this.commandLookup = commandLookup ?? throw new ArgumentNullException(nameof(commandLookup));
            this.argumentExtractor = argumentExtractor ?? throw new ArgumentNullException(nameof(argumentExtractor));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<CommandExtractorResult> ExtractAsync(CommandExtractorContext context)
        {
            // Ensure that extractor options are compatible.
            CommandExtractorResult compatibilityResult = EnsureOptionsCompatibility();
            if (compatibilityResult.IsError)
            {
                return compatibilityResult;
            }

            // Find the command identify by prefix
            OneImlxTryResult<CommandIdentity> commandResult = await commandLookup.TryMatchByPrefixAsync(context.CommandString);
            if (commandResult.IsError)
            {
                return OneImlxResult.NewError<CommandExtractorResult>(commandResult);
            }

            // Make sure we have the result to proceed. Protect bad custom implementations.
            if (commandResult.Result == null)
            {
                return OneImlxResult.NewError<CommandExtractorResult>(Errors.InvalidCommand, logger.FormatAndLog(LogLevel.Error, options.Logging, "The command string did not return an error or match the command prefix. command_string={0}", context.CommandString));
            }

            CommandExtractorResult result = new();
            Arguments? arguments = null;

            // Extract the prefix string so we can process arguments. Arguments are optional for commands
            int prefixEndIndex = context.CommandString.IndexOf(commandResult.Result.Prefix, StringComparison.Ordinal);
            string argString = context.CommandString.Remove(prefixEndIndex, commandResult.Result.Prefix.Length);
            if (!string.IsNullOrWhiteSpace(argString))
            {
                // Make sure there is a separator between the command prefix and arguments
                if (!argString.StartsWith(options.Extractor.Separator, StringComparison.Ordinal))
                {
                    return OneImlxResult.NewError<CommandExtractorResult>(Errors.InvalidCommand, logger.FormatAndLog(LogLevel.Error, options.Logging, "The command separator is missing. command_string={0}", context.CommandString));
                }

                arguments = new();

                string argSplit = string.Concat(options.Extractor.Separator, options.Extractor.ArgumentPrefix);
                string[] args = argString.Split(new string[] { argSplit }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string arg in args)
                {
                    // Restore the arg prefix for the extractor
                    string prefixArg = string.Concat(options.Extractor.ArgumentPrefix, arg);

                    ArgumentExtractorResult argResult = await argumentExtractor.ExtractAsync(new ArgumentExtractorContext(prefixArg, commandResult.Result));
                    if (argResult.IsError)
                    {
                        // Get all errors
                        result.AppendError(argResult);
                    }
                    else
                    {
                        // Protect for bad custom implementation
                        if (argResult.Argument == null)
                        {
                            result.AppendError(OneImlxResult.NewError<CommandExtractorResult>(Errors.InvalidArgument, logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument string did not return an error or extract the argument. argument_string={0}", prefixArg)));
                        }
                        else
                        {
                            arguments.Add(argResult.Argument);
                        }
                    }
                }
            }

            // If there are errors with arguments, don't go any further.
            if (result.IsError)
            {
                return result;
            }

            // Init the command
            Command command = new()
            {
                Id = commandResult.Result.Id,
                Name = commandResult.Result.Name,
                Description = commandResult.Result.Description,
                Arguments = arguments,
            };

            // OK, return the extracted command object.
            result.CommandIdentity = commandResult.Result;
            result.Command = command;
            return result;
        }

        private CommandExtractorResult EnsureOptionsCompatibility()
        {
            // Separator can be whitespace
            if (options.Extractor.Separator == null)
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The command separator is null or not configured.", options.Extractor.Separator);
                return OneImlxResult.NewError<CommandExtractorResult>(Errors.InvalidConfiguration, errorDesc);
            }

            // Command separator and argument separator cannot be same
            if (options.Extractor.Separator.Equals(options.Extractor.ArgumentSeparator, StringComparison.Ordinal))
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The command separator and argument separator cannot be same. separator={0}", options.Extractor.Separator);
                return OneImlxResult.NewError<CommandExtractorResult>(Errors.InvalidConfiguration, errorDesc);
            }

            // Command separator and argument prefix cannot be same
            if (options.Extractor.Separator.Equals(options.Extractor.ArgumentPrefix, StringComparison.Ordinal))
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The command separator and argument prefix cannot be same. separator={0}", options.Extractor.Separator);
                return OneImlxResult.NewError<CommandExtractorResult>(Errors.InvalidConfiguration, errorDesc);
            }

            // Argument separator cannot be null, empty or whitespace
            if (string.IsNullOrWhiteSpace(options.Extractor.ArgumentSeparator))
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument separator cannot be null or whitespace.", options.Extractor.ArgumentSeparator);
                return OneImlxResult.NewError<CommandExtractorResult>(Errors.InvalidConfiguration, errorDesc);
            }

            // Argument separator and argument prefix cannot be same
            if (options.Extractor.ArgumentSeparator.Equals(options.Extractor.ArgumentPrefix, StringComparison.Ordinal))
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument separator and argument prefix cannot be same. separator={0}", options.Extractor.ArgumentSeparator);
                return OneImlxResult.NewError<CommandExtractorResult>(Errors.InvalidConfiguration, errorDesc);
            }

            // Argument prefix cannot be null, empty or whitespace
            if (string.IsNullOrWhiteSpace(options.Extractor.ArgumentPrefix))
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument prefix cannot be null or whitespace.", options.Extractor.ArgumentPrefix);
                return OneImlxResult.NewError<CommandExtractorResult>(Errors.InvalidConfiguration, errorDesc);
            }

            return new();
        }

        private readonly IArgumentExtractor argumentExtractor;
        private readonly ICommandIdentityStore commandLookup;
        private readonly ILogger<SeparatorCommandExtractor> logger;
        private readonly CliOptions options;
    }
}
