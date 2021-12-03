/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Oidc;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Infrastructure;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// The default <c>oneimlx</c> separator based command extractor.
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
            // Check if command string is empty
            if (string.IsNullOrWhiteSpace(context.CommandString))
            {
                return OneImlxResult.NewError<CommandExtractorResult>(Errors.InvalidRequest, logger.FormatAndLog(LogLevel.Error, options.Logging, "The command string is missing in the request."));
            }

            // Find the command identify by prefix
            OneImlxTryResult<CommandIdentity> commandResult = await commandLookup.TryFindByPrefixAsync(context.CommandString);
            if (commandResult.IsError)
            {
                return OneImlxResult.NewError<CommandExtractorResult>(commandResult);
            }

            // Extract the prefix string so we can process arguments. Argument may be optional for commands
            CommandExtractorResult result = new();
            Arguments arguments = new();
            int prefixEndIndex = context.CommandString.IndexOf(commandResult.Result.Prefix, StringComparison.Ordinal);
            string argString = context.CommandString.Remove(prefixEndIndex, commandResult.Result.Prefix.Length);
            if (!string.IsNullOrWhiteSpace(argString))
            {
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
                        arguments.Add(argResult.Argument);
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
                GroupId = commandResult.Result.GroupId,
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
