/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Configuration.Options;
using System.Linq;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalHelpProvider"/> that logs the command help using <see cref="ILogger"/>.
    /// </summary>
    public sealed class TerminalLoggerHelpProvider : ITerminalHelpProvider
    {
        private readonly TerminalOptions terminalOptions;
        private readonly ILogger<TerminalLoggerHelpProvider> _logger;

        /// <summary>
        /// Initializes new instance.
        /// </summary>
        public TerminalLoggerHelpProvider(TerminalOptions terminalOptions, ILogger<TerminalLoggerHelpProvider> logger)
        {
            this.terminalOptions = terminalOptions;
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public Task ProvideHelpAsync(TerminalHelpProviderContext context)
        {
            int indent = 2;
            _logger.LogInformation("Command:");
            _logger.LogInformation(string.Format("{0}{1} ({2}) {3}", new string(' ', indent), context.Command.Id, context.Command.Name, context.Command.Descriptor.Type));
            _logger.LogInformation(string.Format("{0}{1}", new string(' ', indent * 2), context.Command.Description));

            if (context.Command.Descriptor.ArgumentDescriptors != null)
            {
                indent = 2;
                _logger.LogInformation("Arguments:");
                foreach (ArgumentDescriptor argument in context.Command.Descriptor.ArgumentDescriptors)
                {
                    _logger.LogInformation(string.Format("{0}{1} <{2}>", new string(' ', indent), argument.Id, argument.DataType));
                    _logger.LogInformation(string.Format("{0}{1}", new string(' ', indent * 2), argument.Description));
                }
            }

            if (context.Command.Descriptor.OptionDescriptors != null)
            {
                indent = 2;
                _logger.LogInformation("Options:");
                foreach (OptionDescriptor option in context.Command.Descriptor.OptionDescriptors.Values.Distinct())
                {
                    if (option.Alias != null)
                    {
                        _logger.LogInformation(string.Format("{0}{1}{2}, {3}{4} <{5}>", new string(' ', indent), terminalOptions.Parser.OptionPrefix, option.Id, terminalOptions.Parser.OptionAliasPrefix, option.Alias, option.DataType));
                    }
                    else
                    {
                        _logger.LogInformation(string.Format("{0}{1}{2} <{3}>", new string(' ', indent), terminalOptions.Parser.OptionPrefix, option.Id, option.DataType));
                    }

                    _logger.LogInformation(string.Format("{0}{1}", new string(' ', indent * 2), option.Description));
                }
            }

            return Task.CompletedTask;
        }
    }
}