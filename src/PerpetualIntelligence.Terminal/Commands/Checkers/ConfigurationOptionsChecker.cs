/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Checkers
{
    /// <summary>
    /// The default <see cref="IConfigurationOptionsChecker"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="CheckAsync(TerminalOptions)"/> does not return any result. It throws <see cref="TerminalException"/> if
    /// you do not configure an option correctly.
    /// </remarks>
    public class ConfigurationOptionsChecker : IConfigurationOptionsChecker
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ConfigurationOptionsChecker(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Checks the configured <see cref="TerminalOptions"/>.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task CheckAsync(TerminalOptions options)
        {
            ITextHandler textHandler = serviceProvider.GetRequiredService<ITextHandler>();

            // Terminal
            {
                if (options.Id.IsNullOrWhiteSpace())
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The terminal identifier is required.");
                }
            }

            // Driver
            {
                if (options.Driver.Enabled.GetValueOrDefault())
                {
                    // If linked to root command then name is required.
                    if (options.Driver.Name.IsNullOrWhiteSpace())
                    {
                        throw new TerminalException(TerminalErrors.InvalidConfiguration, "The name is required if terminal root is a driver.");
                    }
                }
            }

            // Logging
            {
                if (options.Logging.LoggerIndent <= 0)
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The terminal logger indent cannot be less than or equal to zero. logger_indent={0}", options.Logging.LoggerIndent);
                }
            }

            // Separator
            {
                // Separator cannot be null or empty
                if (options.Extractor.Separator == null || options.Extractor.Separator == string.Empty)
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The command separator cannot be null or empty.", options.Extractor.Separator);
                }

                // Command separator and option prefix cannot be same
                if (textHandler.TextEquals(options.Extractor.Separator, options.Extractor.OptionPrefix))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The command separator and option prefix cannot be same. separator={0}", options.Extractor.Separator);
                }

                // Command separator and option alias prefix cannot be same
                if (textHandler.TextEquals(options.Extractor.Separator, options.Extractor.OptionAliasPrefix))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The command separator and option alias prefix cannot be same. separator={0}", options.Extractor.Separator);
                }
            }

            // Option
            {
                // Option separator can be null or empty
                if (options.Extractor.OptionValueSeparator == null || options.Extractor.OptionValueSeparator == string.Empty)
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The option separator cannot be null or empty.", options.Extractor.Separator);
                }

                // Option prefix cannot be null, empty or whitespace
                if (string.IsNullOrWhiteSpace(options.Extractor.OptionPrefix))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The option prefix cannot be null or whitespace.");
                }

                // Option prefix cannot be more than 3 Unicode characters
                if (textHandler.TextLength(options.Extractor.OptionPrefix) > 3)
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The option prefix cannot be more than 3 Unicode characters. option_prefix={0}", options.Extractor.OptionPrefix);
                }

                // Option alias prefix cannot be null, empty or whitespace
                if (string.IsNullOrWhiteSpace(options.Extractor.OptionAliasPrefix))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The option alias prefix cannot be null or whitespace.");
                }

                // Option prefix cannot be more than 3 Unicode characters
                if (textHandler.TextLength(options.Extractor.OptionAliasPrefix) > 3)
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The option alias prefix cannot be more than 3 Unicode characters. option_alias_prefix={0}", options.Extractor.OptionAliasPrefix);
                }

                // Option separator and option prefix cannot be same
                if (textHandler.TextEquals(options.Extractor.OptionValueSeparator, options.Extractor.OptionPrefix))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The option separator and option prefix cannot be same. separator={0}", options.Extractor.OptionValueSeparator);
                }

                // Option separator and option prefix cannot be same
                if (textHandler.TextEquals(options.Extractor.OptionValueSeparator, options.Extractor.OptionAliasPrefix))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The option separator and option alias prefix cannot be same. separator={0}", options.Extractor.OptionValueSeparator);
                }

                // - FOMAC confusing. Option alias prefix can be same as option prefix but it cannot start with
                // option prefix.
                if (!textHandler.TextEquals(options.Extractor.OptionAliasPrefix, options.Extractor.OptionPrefix) && options.Extractor.OptionAliasPrefix.StartsWith(options.Extractor.OptionPrefix, textHandler.Comparison))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The option alias prefix cannot start with option prefix. prefix={0}", options.Extractor.OptionPrefix);
                }
            }

            // String with in
            {
                // Option prefix cannot be null, empty or whitespace
                if (string.IsNullOrWhiteSpace(options.Extractor.ValueDelimiter))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The value delimiter cannot be null or whitespace.", options.Extractor.ValueDelimiter);
                }

                // with_in cannot be same as OptionPrefix
                if (textHandler.TextEquals(options.Extractor.Separator, options.Extractor.ValueDelimiter))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The value delimiter cannot be same as the separator. delimiter={0}", options.Extractor.ValueDelimiter);
                }

                // with_in cannot be same as OptionPrefix
                if (textHandler.TextEquals(options.Extractor.OptionPrefix, options.Extractor.ValueDelimiter))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The value delimiter cannot be same as the option prefix. delimiter={0}", options.Extractor.ValueDelimiter);
                }

                // with_in cannot be same as OptionAliasPrefix
                if (textHandler.TextEquals(options.Extractor.OptionAliasPrefix, options.Extractor.ValueDelimiter))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The value delimiter cannot be same as the option alias prefix. delimiter={0}", options.Extractor.ValueDelimiter);
                }

                // with_in cannot be same as OptionSeparator
                if (textHandler.TextEquals(options.Extractor.OptionValueSeparator, options.Extractor.ValueDelimiter))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The value delimiter cannot be same as the option value separator. delimiter={0}", options.Extractor.ValueDelimiter);
                }
            }

            return Task.CompletedTask;
        }

        private readonly IServiceProvider serviceProvider;
    }
}