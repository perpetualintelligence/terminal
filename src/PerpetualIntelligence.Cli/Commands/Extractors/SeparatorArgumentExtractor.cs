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
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// The default <c>oneimlx</c> separator based argument extractor.
    /// </summary>
    /// <remarks>The syntax for separator based argument is <c>-{arg}={value}</c> for e.g. -name=testname.</remarks>
    /// <seealso cref="ExtractorOptions.KeyValuePrefix"/>
    /// <seealso cref="ExtractorOptions.KeyValueSeparator"/>
    public class SeparatorArgumentExtractor : IArgumentExtractor
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public SeparatorArgumentExtractor(CliOptions options, ILogger<SeparatorArgumentExtractor> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public Task<ArgumentExtractorResult> ExtractAsync(ArgumentExtractorContext context)
        {
            ArgumentExtractorResult result = new();

            // Null or whitespace
            if (string.IsNullOrWhiteSpace(context.ArgumentString))
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument string is missing in the request. command_name={0} command_id={1}", context.CommandIdentity.Name, context.CommandIdentity.Id);
                result.SetError(new OneImlxError(Errors.InvalidRequest, errorDesc));
            }

            // Check if an app requested a syntax prefix
            if (options.Extractor.KeyValuePrefix != null)
            {
                if (!context.ArgumentString.StartsWith(options.Extractor.KeyValuePrefix.Value))
                {
                    string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument string does not have a valid syntax prefix. command_name={0} command_id={1} arg={2} key_value_prefix={3}", context.CommandIdentity.Name, context.CommandIdentity.Id, context.ArgumentString, options.Extractor.KeyValuePrefix.Value);
                    result.SetError(new OneImlxError(Errors.InvalidRequest, errorDesc));
                }
            }

            // Split by key-value separator
            string[] argSplit = context.ArgumentString.Split(options.Extractor.KeyValueSeparator);
            if (argSplit.Length == 1)
            {
                // Boolean, arg present that means value is true.
                result.Argument = new Argument("<tbd>", argSplit[0].TrimStart(options.Extractor.KeyValuePrefix ?? default, ' '), true, typeof(bool).Name);
            }

            // key-value
            else if (argSplit.Length == 2)
            {
                // Trim all the spaces
                result.Argument = new Argument("{id}", argSplit[0].TrimStart(options.Extractor.KeyValuePrefix ?? default, ' '), argSplit[1]);
            }
            else
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument syntax is not valid. command_name={0} command_id={1} arg={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, context.ArgumentString);
                result.SetError(new OneImlxError(Errors.InvalidRequest, errorDesc));
            }

            return Task.FromResult(result);
        }

        private readonly ILogger<SeparatorArgumentExtractor> logger;
        private readonly CliOptions options;
    }
}
