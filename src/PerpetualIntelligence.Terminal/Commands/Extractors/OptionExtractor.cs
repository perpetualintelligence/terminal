/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Configuration.Options;

using PerpetualIntelligence.Shared.Attributes;
using PerpetualIntelligence.Shared.Exceptions;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    /// <summary>
    /// The default <see cref="IOptionExtractor"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The syntax for a separator based option is <c>{arg}={value}</c> for e.g. <c>name=oneimlx</c>. The syntax has 4 parts:
    /// <list type="number">
    /// <item>
    /// <description><c>-</c> is an option prefix. You can configure it via <see cref="ExtractorOptions.OptionPrefix"/></description>
    /// </item>
    /// <item>
    /// <description>{arg} is an option id. For e.g. <c>name</c></description>
    /// </item>
    /// <item>
    /// <description><c>=</c> is an option separator. You can configure it via <see cref="ExtractorOptions.OptionValueSeparator"/></description>
    /// </item>
    /// <item>
    /// <description>{value} is an option value. For e.g. <c>oneimlx</c></description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="ExtractorOptions.OptionPrefix"/>
    /// <seealso cref="ExtractorOptions.OptionValueSeparator"/>
    [WriteDocumentation]
    public class OptionExtractor : IOptionExtractor
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="textHandler">The text handler.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public OptionExtractor(ITextHandler textHandler, TerminalOptions options, ILogger<OptionExtractor> logger)
        {
            this.textHandler = textHandler;
            this.options = options;
            this.logger = logger;
        }

        /// <summary>
        /// Gets the REGEX pattern to match the option alias with no value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Lets assume that you have configured <see cref="ExtractorOptions.Separator"/> as a single whitespace string
        /// ' ', and <see cref="ExtractorOptions.OptionAliasPrefix"/> as a single dash character string '-'.
        /// </para>
        /// <para>
        /// The default implementation for <see cref="OptionAliasNoValueRegexPattern"/> will match using the following criteria:
        /// </para>
        /// <list type="number">
        /// <item>
        /// <term>'^ *(-)+(.+)$'</term>
        /// <description>Default example REGEX pattern</description>
        /// </item>
        /// <item>
        /// <term>'^'</term>
        /// <description>Matches the beginning of the string</description>
        /// </item>
        /// <item>
        /// <term>' *'</term>
        /// <description>Matches 0 or more <see cref="ExtractorOptions.Separator"/></description>
        /// </item>
        /// <item>
        /// <term>'(-)+'</term>
        /// <description>Create a new capture group and matches 1 or more <see cref="ExtractorOptions.OptionAliasPrefix"/></description>
        /// </item>
        /// <item>
        /// <term>'(.+?)'</term>
        /// <description>Create a new capture group and matches characters (1 or more) except line breaks</description>
        /// </item>
        /// <item>
        /// <term>'$'</term>
        /// <description>Matches the end of the string</description>
        /// </item>
        /// </list>
        /// </remarks>
        public virtual string OptionAliasNoValueRegexPattern
        {
            get
            {
                return $"^[{options.Extractor.Separator}]*({options.Extractor.OptionAliasPrefix})+(.+?)[{options.Extractor.Separator}]*$";
            }
        }

        /// <summary>
        /// Gets the REGEX pattern to match the option id and value using <see cref="ExtractorOptions.OptionPrefix"/>.
        /// </summary>
        public virtual string OptionAliasValueRegexPattern
        {
            get
            {
                return $"^[{options.Extractor.Separator}]*({options.Extractor.OptionAliasPrefix})+(.+?){options.Extractor.OptionValueSeparator}+(.*?)[{options.Extractor.Separator}]*$";
            }
        }

        /// <summary>
        /// Gets the REGEX pattern to match the option alias and value using <see cref="ExtractorOptions.OptionAliasPrefix"/>.
        /// </summary>
        public virtual string OptionIdNoValueRegexPattern
        {
            get
            {
                return $"^[{options.Extractor.Separator}]*({options.Extractor.OptionPrefix})+(.+?)[{options.Extractor.Separator}]*$";
            }
        }

        /// <summary>
        /// Gets the REGEX pattern to match the option alias and value using <see cref="ExtractorOptions.OptionAliasPrefix"/>.
        /// </summary>
        public virtual string OptionIdValueRegexPattern
        {
            get
            {
                return $"^[{options.Extractor.Separator}]*({options.Extractor.OptionPrefix})+(.+?){options.Extractor.OptionValueSeparator}+(.*?)[{options.Extractor.Separator}]*$";
            }
        }

        /// <summary>
        /// Gets the REGEX pattern to match the option alias and value using <see cref="ExtractorOptions.ValueDelimiter"/>.
        /// </summary>
        public virtual string OptionValueWithinRegexPattern
        {
            get
            {
                return $"^{options.Extractor.ValueDelimiter}(.*){options.Extractor.ValueDelimiter}$";
            }
        }

        /// <inheritdoc/>
        public Task<OptionExtractorResult> ExtractAsync(OptionExtractorContext context)
        {
            // Sanity check
            if (context.CommandDescriptor.OptionDescriptors == null)
            {
                throw new ErrorException(TerminalErrors.UnsupportedOption, "The command does not support any options. command_id={0} command_name{1}", context.CommandDescriptor.Id, context.CommandDescriptor.Name);
            }

            string rawOptionString = context.OptionString.Raw;
            bool aliasPrefix = context.OptionString.AliasPrefix;

            // Extract the option and value by default or custom patterns.
            string argIdValueRegex = aliasPrefix ? OptionAliasValueRegexPattern : OptionIdValueRegexPattern;
            Match argIdValueMatch = Regex.Match(rawOptionString, argIdValueRegex);
            string? argIdOrAlias = null;
            string? argValue = null;
            string? argPrefix = null;
            bool matched = false;
            if (argIdValueMatch.Success)
            {
                argPrefix = argIdValueMatch.Groups[1].Value;
                argIdOrAlias = argIdValueMatch.Groups[2].Value;
                argValue = argIdValueMatch.Groups[3].Value;

                // Check if we need to extract the value with_in a token
                if (options.Extractor.ValueDelimiter != null)
                {
                    Match argValueValueDelimiterMatch = Regex.Match(argValue, OptionValueWithinRegexPattern);
                    if (argValueValueDelimiterMatch.Success)
                    {
                        // (.*) captures the group so we can get the string with_in
                        argValue = argValueValueDelimiterMatch.Groups[1].Value;
                    }
                }

                matched = true;
            }
            else
            {
                string argIdOnlyRegex = aliasPrefix ? OptionAliasNoValueRegexPattern : OptionIdNoValueRegexPattern;
                Match argIdOnlyMatch = Regex.Match(rawOptionString, argIdOnlyRegex);
                if (argIdOnlyMatch.Success)
                {
                    argPrefix = argIdOnlyMatch.Groups[1].Value;
                    argIdOrAlias = argIdOnlyMatch.Groups[2].Value;
                    matched = true;
                }
            }

            // Not matched
            if (!matched)
            {
                throw new ErrorException(TerminalErrors.InvalidOption, "The option string is not valid. option_string={0}", rawOptionString);
            }

            // For error handling
            string prefixArgValue = $"{argPrefix}{argIdOrAlias}{options.Extractor.OptionValueSeparator}{argValue}";
            if (argIdOrAlias == null || string.IsNullOrWhiteSpace(argIdOrAlias))
            {
                throw new ErrorException(TerminalErrors.InvalidOption, "The option identifier is null or empty. option_string={0}", prefixArgValue);
            }

            // Find by alias only if configured.
            bool argAndAliasPrefixSame = textHandler.TextEquals(options.Extractor.OptionPrefix, options.Extractor.OptionAliasPrefix);
            bool aliasEnabled = options.Extractor.OptionAlias.GetValueOrDefault();

            // Compatibility check: If OptionAlias is not enabled and the prefix is used to identify by alias then
            // this is an error. If OptionPrefix and OptionAliasPrefix are same then bypass the compatibility check.
            // find it.
            if (!argAndAliasPrefixSame && !aliasEnabled)
            {
                if (textHandler.TextEquals(options.Extractor.OptionAliasPrefix, argPrefix))
                {
                    throw new ErrorException(TerminalErrors.InvalidConfiguration, "The option extraction by alias prefix is not configured. option_string={0}", prefixArgValue);
                }
            }

            // Now find the option by id or alias.
            OptionDescriptor argDescriptor;
            if (aliasEnabled && argAndAliasPrefixSame)
            {
                // If alias is not enabled then we should not be here as we can only find by id. If alias is enabled but
                // the prefix are same then there is no way for us to find the arg precisely, so we first find by id
                // then by it alias or throw.
                argDescriptor = context.CommandDescriptor.OptionDescriptors[argIdOrAlias, true, true];
            }
            else
            {
                // If we are here then we can by precisely find by id or alias as the prefix are different. But we need
                // to now see whether we should find by alias.
                bool findByAlias = aliasPrefix && aliasEnabled;
                argDescriptor = context.CommandDescriptor.OptionDescriptors[argIdOrAlias, findByAlias];
            }

            // Key only (treat it as a boolean) value=true
            if (nameof(Boolean).Equals(argDescriptor.DataType))
            {
                // The value will not be white space because we have already removed all the separators.
                string value = (argValue == null || textHandler.TextEquals(argValue, string.Empty)) ? true.ToString() : argValue;
                return Task.FromResult(new OptionExtractorResult(new Option(argDescriptor, value)));
            }
            else
            {
                if (argValue == null)
                {
                    throw new ErrorException(TerminalErrors.InvalidOption, "The option value is missing. option_string={0}", prefixArgValue);
                }

                return Task.FromResult(new OptionExtractorResult(new Option(argDescriptor, argValue)));
            }
        }

        private readonly ILogger<OptionExtractor> logger;
        private readonly TerminalOptions options;
        private readonly ITextHandler textHandler;
    }
}
