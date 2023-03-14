/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
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

                // Command separator and argument prefix cannot be same
                if (textHandler.TextEquals(options.Extractor.Separator, options.Extractor.ArgumentPrefix))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The command separator and argument prefix cannot be same. separator={0}", options.Extractor.Separator);
                }

                // Command separator and argument alias prefix cannot be same
                if (textHandler.TextEquals(options.Extractor.Separator, options.Extractor.ArgumentAliasPrefix))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The command separator and argument alias prefix cannot be same. separator={0}", options.Extractor.Separator);
                }
            }

            // Argument
            {
                // Argument separator can be null or empty
                if (options.Extractor.ArgumentValueSeparator == null || options.Extractor.ArgumentValueSeparator == string.Empty)
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The argument separator cannot be null or empty.", options.Extractor.Separator);
                }

                // Argument prefix cannot be null, empty or whitespace
                if (string.IsNullOrWhiteSpace(options.Extractor.ArgumentPrefix))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The argument prefix cannot be null or whitespace.");
                }

                // Argument prefix cannot be more than 3 Unicode characters
                if (textHandler.TextLength(options.Extractor.ArgumentPrefix) > 3)
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The argument prefix cannot be more than 3 Unicode characters. argument_prefix={0}", options.Extractor.ArgumentPrefix);
                }

                // Argument alias prefix cannot be null, empty or whitespace
                if (string.IsNullOrWhiteSpace(options.Extractor.ArgumentAliasPrefix))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The argument alias prefix cannot be null or whitespace.");
                }

                // Argument prefix cannot be more than 3 Unicode characters
                if (textHandler.TextLength(options.Extractor.ArgumentAliasPrefix) > 3)
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The argument alias prefix cannot be more than 3 Unicode characters. argument_alias_prefix={0}", options.Extractor.ArgumentAliasPrefix);
                }

                // Argument separator and argument prefix cannot be same
                if (textHandler.TextEquals(options.Extractor.ArgumentValueSeparator, options.Extractor.ArgumentPrefix))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The argument separator and argument prefix cannot be same. separator={0}", options.Extractor.ArgumentValueSeparator);
                }

                // Argument separator and argument prefix cannot be same
                if (textHandler.TextEquals(options.Extractor.ArgumentValueSeparator, options.Extractor.ArgumentAliasPrefix))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The argument separator and argument alias prefix cannot be same. separator={0}", options.Extractor.ArgumentValueSeparator);
                }

                // - FOMAC confusing. Argument alias prefix can be same as argument prefix but it cannot start with
                // argument prefix.
                if (!textHandler.TextEquals(options.Extractor.ArgumentAliasPrefix, options.Extractor.ArgumentPrefix) && options.Extractor.ArgumentAliasPrefix.StartsWith(options.Extractor.ArgumentPrefix, textHandler.Comparison))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The argument alias prefix cannot start with argument prefix. prefix={0}", options.Extractor.ArgumentPrefix);
                }
            }

            // String with in
            {
                // Argument prefix cannot be null, empty or whitespace
                if (options.Extractor.ArgumentValueWithIn != null && options.Extractor.ArgumentValueWithIn.All(e => char.IsWhiteSpace(e)))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The string with_in token cannot be whitespace.", options.Extractor.ArgumentValueWithIn);
                }

                // with_in cannot be same as ArgumentPrefix
                if (textHandler.TextEquals(options.Extractor.Separator, options.Extractor.ArgumentValueWithIn))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The string with_in token and separator cannot be same. with_in={0}", options.Extractor.ArgumentValueWithIn);
                }

                // with_in cannot be same as ArgumentPrefix
                if (textHandler.TextEquals(options.Extractor.ArgumentPrefix, options.Extractor.ArgumentValueWithIn))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The string with_in token and argument prefix cannot be same. with_in={0}", options.Extractor.ArgumentValueWithIn);
                }

                // with_in cannot be same as ArgumentAliasPrefix
                if (textHandler.TextEquals(options.Extractor.ArgumentAliasPrefix, options.Extractor.ArgumentValueWithIn))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The string with_in token and argument alias prefix cannot be same. with_in={0}", options.Extractor.ArgumentValueWithIn);
                }

                // with_in cannot be same as ArgumentSeparator
                if (textHandler.TextEquals(options.Extractor.ArgumentValueSeparator, options.Extractor.ArgumentValueWithIn))
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The string with_in token and argument separator cannot be same. with_in={0}", options.Extractor.ArgumentValueWithIn);
                }
            }

            // Default argument and values
            {
                IDefaultArgumentProvider? defaultArgumentProvider = serviceProvider.GetService<IDefaultArgumentProvider>();
                IDefaultArgumentValueProvider? defaultArgumentValueProvider = serviceProvider.GetService<IDefaultArgumentValueProvider>();

                // Command default argument provider is missing
                if (options.Extractor.DefaultArgument.GetValueOrDefault() && defaultArgumentProvider == null)
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The command default argument provider is missing in the service collection. provider_type={0}", typeof(IDefaultArgumentProvider).FullName);
                }

                // Argument default value provider is missing
                if (options.Extractor.DefaultArgumentValue.GetValueOrDefault() && defaultArgumentValueProvider == null)
                {
                    throw new ErrorException(Errors.InvalidConfiguration, "The argument default value provider is missing in the service collection. provider_type={0}", typeof(IDefaultArgumentValueProvider).FullName);
                }
            }

            return Task.CompletedTask;
        }

        private readonly IServiceProvider serviceProvider;
    }
}