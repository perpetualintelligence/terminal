/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Extensions;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// The separator based argument extractor.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The syntax for a separator based argument is <c>-{arg}={value}</c> for e.g. <c>-name=oneimlx</c>. The syntax has
    /// 4 parts:
    /// <list type="number">
    /// <item>
    /// <description><c>-</c> is an argument prefix. You can configure it via <see cref="ExtractorOptions.ArgumentPrefix"/></description>
    /// </item>
    /// <item>
    /// <description>{arg} is an argument id. For e.g. <c>name</c></description>
    /// </item>
    /// <item>
    /// <description><c>=</c> is an argument separator. You can configure it via <see cref="ExtractorOptions.ArgumentSeparator"/></description>
    /// </item>
    /// <item>
    /// <description>{value} is an argument value. For e.g. <c>oneimlx</c></description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
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
            // Null command descriptor
            if (context.CommandDescriptor == null)
            {
                throw new ErrorException(Errors.InvalidRequest, "The command descriptor is missing in the request. extractor={0}", GetType().FullName);
            }

            // Null or whitespace
            if (string.IsNullOrWhiteSpace(context.ArgumentString))
            {
                throw new ErrorException(Errors.InvalidArgument, "The argument string is missing in the request. command_name={0} command_id={1} extractor={2}", context.CommandDescriptor.Name, context.CommandDescriptor.Id, GetType().FullName);
            }

            // Check if an app requested a syntax prefix
            string argumentString = context.ArgumentString;
            if (options.Extractor.ArgumentPrefix != null)
            {
                if (context.ArgumentString.StartsWith(options.Extractor.ArgumentPrefix, System.StringComparison.Ordinal))
                {
                    // Trim the argument prefix
                    argumentString = context.ArgumentString.TrimStart(options.Extractor.ArgumentPrefix);
                }
                else
                {
                    // Must start with argument prefix
                    throw new ErrorException(Errors.InvalidArgument, "The argument string does not have a valid prefix. command_name={0} command_id={1} arg={2} prefix={3}", context.CommandDescriptor.Name, context.CommandDescriptor.Id, context.ArgumentString, options.Extractor.ArgumentPrefix);
                }
            }

            // Split by key-value separator
            int sepIdx = argumentString.IndexOf(options.Extractor.ArgumentSeparator, StringComparison.Ordinal);
            string argId;
            string? argValue = null;
            if (sepIdx < 0)
            {
                // Boolean (Non value arg)
                argId = argumentString;
            }
            else
            {
                argId = argumentString.Substring(0, sepIdx);
                argValue = argumentString.TrimStart(argId).TrimStart(options.Extractor.ArgumentSeparator);
            }

            // FOMAC
            //string[] argSplit = argumentString.Split(options.Extractor.ArgumentSeparator);
            //if (argSplit.Length > 2)
            //{
            //    // Invalid syntax
            //    string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument syntax is not valid. command_name={0} command_id={1} argument_string={2}", context.CommandDescriptor.Name, context.CommandDescriptor.Id, context.ArgumentString);
            //    return Task.FromResult(OneImlxResult.NewError<ArgumentExtractorResult>(Errors.InvalidArgument, errorDesc));
            //}

            // The split key cannot be null or empty
            if (string.IsNullOrWhiteSpace(argId))
            {
                throw new ErrorException(Errors.InvalidArgument, "The argument id is null or empty. command_name={0} command_id={1} arg={2}", context.CommandDescriptor.Name, context.CommandDescriptor.Id, context.ArgumentString);
            }

            // Now find the argument by key or name
            ArgumentDescriptor argFindResult = FindAttributeOrThrow(context.CommandDescriptor, argId);

            // Key only (treat it as a boolean) value=true
            if (argValue == null)
            {
                return Task.FromResult(new ArgumentExtractorResult(new Argument(argFindResult, true)));
            }
            else
            {
                // key-value TODO Trim all the prefix
                return Task.FromResult(new ArgumentExtractorResult(new Argument(argFindResult, argValue)));
            }
        }

        private ArgumentDescriptor FindAttributeOrThrow(CommandDescriptor commandDescriptor, string argId)
        {
            bool found = commandDescriptor.TryGetArgumentDescriptor(argId, out ArgumentDescriptor? arg);
            if (found)
            {
                return arg;
            }
            else
            {
                throw new ErrorException(Errors.UnsupportedArgument, "The argument is not supported. command_name={0} command_id={1} argument={2}", commandDescriptor.Name, commandDescriptor.Id, argId);
            }
        }

        private readonly ILogger<SeparatorArgumentExtractor> logger;
        private readonly CliOptions options;
    }
}
