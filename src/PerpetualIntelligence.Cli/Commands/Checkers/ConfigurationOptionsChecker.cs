/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.DependencyInjection;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Providers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Shared.Exceptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    /// <summary>
    /// The default <see cref="IConfigurationOptionsChecker"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="CheckAsync(CliOptions)"/> does not return any result. It throws <see cref="ErrorException"/> if
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
        /// Checks the configured <see cref="CliOptions"/>.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task CheckAsync(CliOptions options)
        {
            ITextHandler textHandler = serviceProvider.GetRequiredService<ITextHandler>();

            // Logging
            {
                if (options.Logging.LoggerIndent <= 0)
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The terminal logger indent cannot be less than or equal to zero. logger_indent={0}", options.Logging.LoggerIndent);
                }
            }

            // Separator
            {
                // Separator cannot be null or empty
                if (options.Extractor.Separator == null || options.Extractor.Separator == string.Empty)
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The command separator cannot be null or empty.", options.Extractor.Separator);
                }

                // Command separator and option prefix cannot be same
                if (textHandler.TextEquals(options.Extractor.Separator, options.Extractor.OptionPrefix))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The command separator and option prefix cannot be same. separator={0}", options.Extractor.Separator);
                }

                // Command separator and option alias prefix cannot be same
                if (textHandler.TextEquals(options.Extractor.Separator, options.Extractor.OptionAliasPrefix))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The command separator and option alias prefix cannot be same. separator={0}", options.Extractor.Separator);
                }
            }

            // Option
            {
                // Option separator can be null or empty
                if (options.Extractor.OptionValueSeparator == null || options.Extractor.OptionValueSeparator == string.Empty)
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The option separator cannot be null or empty.", options.Extractor.Separator);
                }

                // Option prefix cannot be null, empty or whitespace
                if (string.IsNullOrWhiteSpace(options.Extractor.OptionPrefix))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The option prefix cannot be null or whitespace.");
                }

                // Option prefix cannot be more than 3 Unicode characters
                if (textHandler.TextLength(options.Extractor.OptionPrefix) > 3)
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The option prefix cannot be more than 3 Unicode characters. argument_prefix={0}", options.Extractor.OptionPrefix);
                }

                // Option alias prefix cannot be null, empty or whitespace
                if (string.IsNullOrWhiteSpace(options.Extractor.OptionAliasPrefix))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The option alias prefix cannot be null or whitespace.");
                }

                // Option prefix cannot be more than 3 Unicode characters
                if (textHandler.TextLength(options.Extractor.OptionAliasPrefix) > 3)
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The option alias prefix cannot be more than 3 Unicode characters. argument_alias_prefix={0}", options.Extractor.OptionAliasPrefix);
                }

                // Option separator and option prefix cannot be same
                if (textHandler.TextEquals(options.Extractor.OptionValueSeparator, options.Extractor.OptionPrefix))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The option separator and option prefix cannot be same. separator={0}", options.Extractor.OptionValueSeparator);
                }

                // Option separator and option prefix cannot be same
                if (textHandler.TextEquals(options.Extractor.OptionValueSeparator, options.Extractor.OptionAliasPrefix))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The option separator and option alias prefix cannot be same. separator={0}", options.Extractor.OptionValueSeparator);
                }

                // - FOMAC confusing. Option alias prefix can be same as option prefix but it cannot start with
                // option prefix.
                if (!textHandler.TextEquals(options.Extractor.OptionAliasPrefix, options.Extractor.OptionPrefix) && options.Extractor.OptionAliasPrefix.StartsWith(options.Extractor.OptionPrefix, textHandler.Comparison))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The option alias prefix cannot start with option prefix. prefix={0}", options.Extractor.OptionPrefix);
                }
            }

            // String with in
            {
                // Option prefix cannot be null, empty or whitespace
                if (options.Extractor.OptionValueWithIn != null && options.Extractor.OptionValueWithIn.All(e => char.IsWhiteSpace(e)))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The string with_in token cannot be whitespace.", options.Extractor.OptionValueWithIn);
                }

                // with_in cannot be same as ArgumentPrefix
                if (textHandler.TextEquals(options.Extractor.Separator, options.Extractor.OptionValueWithIn))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The string with_in token and separator cannot be same. with_in={0}", options.Extractor.OptionValueWithIn);
                }

                // with_in cannot be same as ArgumentPrefix
                if (textHandler.TextEquals(options.Extractor.OptionPrefix, options.Extractor.OptionValueWithIn))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The string with_in token and option prefix cannot be same. with_in={0}", options.Extractor.OptionValueWithIn);
                }

                // with_in cannot be same as ArgumentAliasPrefix
                if (textHandler.TextEquals(options.Extractor.OptionAliasPrefix, options.Extractor.OptionValueWithIn))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The string with_in token and option alias prefix cannot be same. with_in={0}", options.Extractor.OptionValueWithIn);
                }

                // with_in cannot be same as ArgumentSeparator
                if (textHandler.TextEquals(options.Extractor.OptionValueSeparator, options.Extractor.OptionValueWithIn))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The string with_in token and option separator cannot be same. with_in={0}", options.Extractor.OptionValueWithIn);
                }
            }

            // Default option and values
            {
                IDefaultOptionProvider? defaultArgumentProvider = serviceProvider.GetService<IDefaultOptionProvider>();
                IDefaultOptionValueProvider? defaultArgumentValueProvider = serviceProvider.GetService<IDefaultOptionValueProvider>();

                // Command default option provider is missing
                if (options.Extractor.DefaultOption.GetValueOrDefault() && defaultArgumentProvider == null)
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The command default option provider is missing in the service collection. provider_type={0}", typeof(IDefaultOptionProvider).Name);
                }

                // Option default value provider is missing
                if (options.Extractor.DefaultOptionValue.GetValueOrDefault() && defaultArgumentValueProvider == null)
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The option default value provider is missing in the service collection. provider_type={0}", typeof(IDefaultOptionValueProvider).Name);
                }
            }

            return Task.CompletedTask;
        }

        private readonly IServiceProvider serviceProvider;
    }
}