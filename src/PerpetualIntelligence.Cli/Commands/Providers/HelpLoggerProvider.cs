/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Providers
{
    /// <summary>
    /// The default <see cref="IHelpProvider"/> that logs the command help using <see cref="ILogger"/>.
    /// </summary>
    public sealed class HelpLoggerProvider : IHelpProvider
    {
        /// <summary>
        /// Initializes new instance.
        /// </summary>
        public HelpLoggerProvider(ILogger<HelpLoggerProvider> logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// The logger.
        /// </summary>
        public ILogger<HelpLoggerProvider> Logger { get; }

        /// <inheritdoc/>
        public Task ProvideHelpAsync(HelpProviderContext context)
        {
            Logger.LogInformation("Command:");
            Logger.LogInformation("    " + context.Command.Descriptor.Name);
            Logger.LogInformation(context.Command.Descriptor.Description);

            if (context.Command.Descriptor.ArgumentDescriptors != null)
            {
                foreach (OptionDescriptor argument in context.Command.Descriptor.ArgumentDescriptors)
                {
                    if (argument.Alias != null)
                    {
                        Logger.LogInformation("{0} ({1}) - {2}", argument.Id, argument.Alias, argument.Description);
                    }
                    else
                    {
                        Logger.LogInformation("{0} - {1}", argument.Id, argument.Description);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}