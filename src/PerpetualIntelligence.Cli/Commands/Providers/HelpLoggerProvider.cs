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

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task ProvideHelpAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}