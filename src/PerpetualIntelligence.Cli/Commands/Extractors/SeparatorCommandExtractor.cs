/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Infrastructure;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// The <c>cli</c> separator based command extractor.
    /// </summary>
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

            // Extract the prefix string so we can process arguments. Arguments are optional for commands
            CommandExtractorResult result = new();
            Arguments? arguments = null;
            string[] cmdSplits = context.CommandString.Split(options.Extractor.Separator);

            int prefixEndIndex = context.CommandString.IndexOf(commandResult.Result.Prefix, StringComparison.Ordinal);
            string argString = context.CommandString.Remove(prefixEndIndex, commandResult.Result.Prefix.Length);
            if (!string.IsNullOrWhiteSpace(argString))
            {
                // Init the arguments collection
                arguments = new();

                // Split by separator
                string[] args = argString.Split(options.Extractor.Separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (string arg in args)
                {
                    ArgumentExtractorResult argResult = await argumentExtractor.ExtractAsync(new ArgumentExtractorContext(arg, commandResult.Result));
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
                            result.AppendError(OneImlxResult.NewError<CommandExtractorResult>(Errors.InvalidArgument, logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument string did not return an error or extract the argument. argument_string={0}", arg)));
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

        private readonly IArgumentExtractor argumentExtractor;
        private readonly ICommandIdentityStore commandLookup;
        private readonly ILogger<SeparatorCommandExtractor> logger;
        private readonly CliOptions options;
    }
}
