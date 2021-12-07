/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Integration.Configuration.Options;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Infrastructure;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// The default <c>oneimlx</c> separator based argument extractor.
    /// </summary>
    /// <remarks>The syntax for separator based argument is <c>-{arg}={value}</c> for e.g. -name=testname.</remarks>
    /// <seealso cref="ExtractorOptions.ArgumentPrefix"/>
    /// <seealso cref="ExtractorOptions.ArgumentSeparator"/>
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
            // Null command identity
            if (context.CommandIdentity == null)
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The command identity is missing in the request. extractor={0}", GetType().FullName);
                return Task.FromResult(OneImlxResult.NewError<ArgumentExtractorResult>(Errors.InvalidRequest, errorDesc));
            }

            // Null or whitespace
            if (string.IsNullOrWhiteSpace(context.ArgumentString))
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument string is missing in the request. command_name={0} command_id={1} extractor={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, GetType().FullName);
                return Task.FromResult(OneImlxResult.NewError<ArgumentExtractorResult>(Errors.InvalidArgument, errorDesc));
            }

            // Check if an app requested a syntax prefix
            string argumentString = context.ArgumentString;
            if (options.Extractor.ArgumentPrefix != null)
            {
                if (!context.ArgumentString.StartsWith(options.Extractor.ArgumentPrefix, System.StringComparison.Ordinal))
                {
                    string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument string does not have a valid prefix. command_name={0} command_id={1} arg={2} prefix={3}", context.CommandIdentity.Name, context.CommandIdentity.Id, context.ArgumentString, options.Extractor.ArgumentPrefix);
                    return Task.FromResult(OneImlxResult.NewError<ArgumentExtractorResult>(Errors.InvalidArgument, errorDesc));
                }
                else
                {
                    // Trim the argument prefix
                    argumentString = context.ArgumentString.TrimStart(options.Extractor.ArgumentPrefix);
                }
            }

            // Split by key-value separator
            ArgumentExtractorResult result = new();
            int sepIdx = argumentString.IndexOf(options.Extractor.ArgumentSeparator, StringComparison.Ordinal);
            string argName;
            string? argValue = null;
            if (sepIdx < 0)
            {
                // Boolean (Non value arg)
                argName = argumentString;
            }
            else
            {
                argName = argumentString.Substring(0, sepIdx);
                argValue = argumentString.TrimStart(argName).TrimStart(options.Extractor.ArgumentSeparator);
            }

            //string[] argSplit = argumentString.Split(options.Extractor.ArgumentSeparator);
            //if (argSplit.Length > 2)
            //{
            //    // Invalid syntax
            //    string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument syntax is not valid. command_name={0} command_id={1} argument_string={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, context.ArgumentString);
            //    return Task.FromResult(OneImlxResult.NewError<ArgumentExtractorResult>(Errors.InvalidArgument, errorDesc));
            //}

            // The split key cannot be null or empty
            if (string.IsNullOrWhiteSpace(argName))
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument name is null or empty. command_name={0} command_id={1} arg={2}", context.CommandIdentity.Name, context.CommandIdentity.Id, context.ArgumentString);
                return Task.FromResult(OneImlxResult.NewError<ArgumentExtractorResult>(Errors.InvalidArgument, errorDesc));
            }

            // Now find the argument by key or name
            var argFindResult = TryFindAttributeByName(context.CommandIdentity, argName);
            if (argFindResult.IsError)
            {
                return Task.FromResult(OneImlxResult.NewError<ArgumentExtractorResult>(argFindResult));
            }

            // Key only (treat it as a boolean) value=true
            if (argValue == null)
            {
                result.Argument = new Argument(argFindResult.Result!, true);
            }
            else
            {
                // key-value TODO Trim all the prefix
                result.Argument = new Argument(argFindResult.Result!, argValue);
            }

            return Task.FromResult(result);
        }

        private OneImlxTryResult<ArgumentIdentity> TryFindAttributeByName(CommandIdentity commandIdentity, string argName)
        {
            if (commandIdentity.ArgumentIdentities != null)
            {
                try
                {
                    ArgumentIdentity? arg = commandIdentity.ArgumentIdentities.FindByName(argName);
                    if (arg == null)
                    {
                        string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The command does not support the specified argument. command_name={0} command_id={1} argument={2}", commandIdentity.Name, commandIdentity.Id, argName);
                        return OneImlxResult.NewError<OneImlxTryResult<ArgumentIdentity>>(Errors.UnsupportedArgument, errorDesc);
                    }
                    else
                    {
                        return new OneImlxTryResult<ArgumentIdentity>(arg);
                    }
                }
                catch (NotUniqueException)
                {
                    string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The command does not support the same multiple arguments. command_name={0} command_id={1} argument={2}", commandIdentity.Name, commandIdentity.Id, argName);
                    return OneImlxResult.NewError<OneImlxTryResult<ArgumentIdentity>>(Errors.UnsupportedArgument, errorDesc);
                }
            }
            else
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The command does not support any argument. command_name={0} command_id={1}", commandIdentity.Name, commandIdentity.Id);
                return OneImlxResult.NewError<OneImlxTryResult<ArgumentIdentity>>(Errors.UnsupportedArgument, errorDesc);
            }
        }

        private readonly ILogger<SeparatorArgumentExtractor> logger;
        private readonly CliOptions options;
    }
}
