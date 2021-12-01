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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The default command extractor.
    /// </summary>
    public class StringArgumentsExtractor : IArgumentsExtractor
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public StringArgumentsExtractor(CliOptions options, ILogger<StringCommandExtractor> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public Task<ArgumentsExtractorResult> ExtractAsync(ArgumentsExtractorContext context)
        {
            ArgumentsExtractorResult result = new() { Arguments = new Arguments() };

            // Check if command string is empty
            if (string.IsNullOrWhiteSpace(context.Arguments))
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The arguments string is missing in the request.");
                result.SetError(Errors.InvalidRequest, errorDesc);
                return Task.FromResult(result);
            }

            // Split by space
            string[] args = context.Arguments.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Make sure all the arguments are valid.
            foreach (string arg in args)
            {
                // Check argument syntax
                OneImlxTryResult<KeyValuePair<string, object?>> tryKvp = TryCheckArgument(context.CommandIdentity, arg);
                if (tryKvp.IsError)
                {
                    result.SyncError(tryKvp);
                    return Task.FromResult(result);
                }

                // Push argument
                result.Arguments.Add(tryKvp.Result.Key, tryKvp.Result.Value);
            }

            return Task.FromResult(result);
        }

        private OneImlxTryResult<KeyValuePair<string, object?>> TryCheckArgument(CommandIdentity commandIdentity, string arg)
        {
            OneImlxTryResult<KeyValuePair<string, object?>> tryResult = new();

            // Split by space
            string[] argSplit = arg.Split(new char[] { '=' });
            if (argSplit.Length != 2)
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument format is not valid. command={0} arg={1}", commandIdentity.Name, arg);
                tryResult.SetError(new OneImlxError(Errors.InvalidRequest, errorDesc));
            }

            // Trim all the spaces
            tryResult.Result = new KeyValuePair<string, object?>(argSplit[0].Trim(new char[] { '-', ' ' }), argSplit[1].Trim());
            return tryResult;
        }

        private readonly ILogger<StringCommandExtractor> logger;
        private readonly CliOptions options;
    }
}
