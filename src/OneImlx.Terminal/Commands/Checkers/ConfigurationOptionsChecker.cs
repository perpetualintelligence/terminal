/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OneImlx.Shared.Extensions;
using OneImlx.Terminal.Configuration.Options;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Commands.Checkers
{
    /// <summary>
    /// The default <see cref="IConfigurationOptionsChecker"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="CheckAsync(TerminalOptions)"/> does not return any result. It throws
    /// <see cref="TerminalException"/> if you do not configure an option correctly.
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
            ITerminalTextHandler textHandler = serviceProvider.GetRequiredService<ITerminalTextHandler>();

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

            // Separator
            {
                // Separator cannot be null or empty
                if (options.Parser.Separator == null || options.Parser.Separator == string.Empty)
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The command separator cannot be null or empty.", options.Parser.Separator);
                }

                // Command separator and option prefix cannot be same
                if (textHandler.TextEquals(options.Parser.Separator, options.Parser.OptionPrefix))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The command separator and option prefix cannot be same. separator={0}", options.Parser.Separator);
                }

                // Command separator and option alias prefix cannot be same
                if (textHandler.TextEquals(options.Parser.Separator, options.Parser.OptionAliasPrefix))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The command separator and option alias prefix cannot be same. separator={0}", options.Parser.Separator);
                }
            }

            // Option
            {
                // Option separator can be null or empty
                if (options.Parser.OptionValueSeparator == null || options.Parser.OptionValueSeparator == string.Empty)
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The option separator cannot be null or empty.", options.Parser.Separator);
                }

                // Option prefix cannot be null, empty or whitespace
                if (string.IsNullOrWhiteSpace(options.Parser.OptionPrefix))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The option prefix cannot be null or whitespace.");
                }

                // Option prefix cannot be more than 3 Unicode characters
                if (textHandler.TextLength(options.Parser.OptionPrefix) > 3)
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The option prefix cannot be more than 3 Unicode characters. option_prefix={0}", options.Parser.OptionPrefix);
                }

                // Option alias prefix cannot be null, empty or whitespace
                if (string.IsNullOrWhiteSpace(options.Parser.OptionAliasPrefix))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The option alias prefix cannot be null or whitespace.");
                }

                // Option prefix cannot be more than 3 Unicode characters
                if (textHandler.TextLength(options.Parser.OptionAliasPrefix) > 3)
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The option alias prefix cannot be more than 3 Unicode characters. option_alias_prefix={0}", options.Parser.OptionAliasPrefix);
                }

                // Option separator and option prefix cannot be same
                if (textHandler.TextEquals(options.Parser.OptionValueSeparator, options.Parser.OptionPrefix))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The option separator and option prefix cannot be same. separator={0}", options.Parser.OptionValueSeparator);
                }

                // Option separator and option prefix cannot be same
                if (textHandler.TextEquals(options.Parser.OptionValueSeparator, options.Parser.OptionAliasPrefix))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The option separator and option alias prefix cannot be same. separator={0}", options.Parser.OptionValueSeparator);
                }

                // Option alias prefix can be same as option prefix but it cannot start with option prefix. e.g
                // --configuration -c is valid but --configuration and --c is not
                if (!textHandler.TextEquals(options.Parser.OptionAliasPrefix, options.Parser.OptionPrefix) && options.Parser.OptionAliasPrefix.StartsWith(options.Parser.OptionPrefix, textHandler.Comparison))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The option alias prefix cannot start with option prefix. prefix={0}", options.Parser.OptionPrefix);
                }
            }

            // Local Delimiter
            {
                // Option prefix cannot be null, empty or whitespace
                if (string.IsNullOrWhiteSpace(options.Parser.ValueDelimiter))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The value delimiter cannot be null or whitespace.");
                }

                // with_in cannot be same as OptionPrefix
                if (textHandler.TextEquals(options.Parser.Separator, options.Parser.ValueDelimiter))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The value delimiter cannot be same as the separator. delimiter={0}", options.Parser.ValueDelimiter);
                }

                // with_in cannot be same as OptionPrefix
                if (textHandler.TextEquals(options.Parser.OptionPrefix, options.Parser.ValueDelimiter))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The value delimiter cannot be same as the option prefix. delimiter={0}", options.Parser.ValueDelimiter);
                }

                // with_in cannot be same as OptionAliasPrefix
                if (textHandler.TextEquals(options.Parser.OptionAliasPrefix, options.Parser.ValueDelimiter))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The value delimiter cannot be same as the option alias prefix. delimiter={0}", options.Parser.ValueDelimiter);
                }

                // with_in cannot be same as OptionSeparator
                if (textHandler.TextEquals(options.Parser.OptionValueSeparator, options.Parser.ValueDelimiter))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The value delimiter cannot be same as the option value separator. delimiter={0}", options.Parser.ValueDelimiter);
                }
            }

            // Remote Delimiter
            {
                // Remote command delimiter cannot be null, empty or whitespace
                if (string.IsNullOrWhiteSpace(options.Router.RemoteCommandDelimiter))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The remote command delimiter cannot be null or whitespace.");
                }

                // Remote message delimiter cannot be null, empty or whitespace
                if (string.IsNullOrWhiteSpace(options.Router.RemoteBatchDelimiter))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The remote message delimiter cannot be null or whitespace.");
                }

                // Remote command delimiter cannot be same as remote message delimiter
                if (textHandler.TextEquals(options.Router.RemoteCommandDelimiter, options.Router.RemoteBatchDelimiter))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The remote command delimiter and remote message delimiter cannot be same. delimiter={0}", options.Router.RemoteCommandDelimiter);
                }
            }

            return Task.CompletedTask;
        }

        private readonly IServiceProvider serviceProvider;
    }
}
