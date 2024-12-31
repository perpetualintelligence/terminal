/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

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
                if (options.Driver.Enabled)
                {
                    // If linked to root command then name is required.
                    if (options.Driver.RootId.IsNullOrWhiteSpace())
                    {
                        throw new TerminalException(TerminalErrors.InvalidConfiguration, "The root is required for driver programs.");
                    }
                }
            }

            // Separator
            {
                // Separator cannot be null or empty
                if (options.Parser.Separator == default)
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The command separator cannot be null or empty.", options.Parser.Separator);
                }

                // Command separator and option prefix cannot be same
                if (textHandler.CharEquals(options.Parser.Separator, options.Parser.OptionPrefix))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The command separator and option prefix cannot be same. separator={0}", options.Parser.Separator);
                }
            }

            // Option
            {
                // Option separator can be null or empty
                if (options.Parser.OptionValueSeparator == default)
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The option separator cannot be null or empty.", options.Parser.Separator);
                }

                // Option prefix cannot be default
                if (options.Parser.OptionPrefix == default)
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The option prefix cannot be default.");
                }

                // Option separator and option prefix cannot be same
                if (textHandler.CharEquals(options.Parser.OptionValueSeparator, options.Parser.OptionPrefix))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The option separator and option prefix cannot be same. separator={0}", options.Parser.OptionValueSeparator);
                }
            }

            // Value Delimiter
            {
                // Value delimiter cannot be null, empty or whitespace
                if (options.Parser.ValueDelimiter == default || options.Parser.ValueDelimiter == ' ')
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The value delimiter cannot be null or whitespace.");
                }

                // with_in cannot be same as OptionPrefix
                if (textHandler.CharEquals(options.Parser.ValueDelimiter, options.Parser.Separator))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The value delimiter cannot be same as the separator. delimiter={0}", options.Parser.ValueDelimiter);
                }

                // with_in cannot be same as OptionPrefix
                if (textHandler.CharEquals(options.Parser.ValueDelimiter, options.Parser.OptionPrefix))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The value delimiter cannot be same as the option prefix. delimiter={0}", options.Parser.ValueDelimiter);
                }

                // with_in cannot be same as OptionSeparator
                if (textHandler.CharEquals(options.Parser.ValueDelimiter, options.Parser.OptionValueSeparator))
                {
                    throw new TerminalException(TerminalErrors.InvalidConfiguration, "The value delimiter cannot be same as the option value separator. delimiter={0}", options.Parser.ValueDelimiter);
                }
            }

            return Task.CompletedTask;
        }

        private readonly IServiceProvider serviceProvider;
    }
}
