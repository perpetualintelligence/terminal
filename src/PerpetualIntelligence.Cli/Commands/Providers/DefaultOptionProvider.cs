/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;

using PerpetualIntelligence.Shared.Exceptions;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Providers
{
    /// <summary>
    /// The default option provider.
    /// </summary>
    /// <seealso cref="CommandDescriptor.DefaultArgument"/>
    public class DefaultOptionProvider : IDefaultOptionProvider
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public DefaultOptionProvider(CliOptions options, ILogger<DefaultOptionProvider> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        /// <summary>
        /// Provides default values for all the command options.
        /// </summary>
        /// <param name="context">The option default value provider context.</param>
        /// <returns>The <see cref="DefaultOptionValueProviderResult"/> instance that contains the default values.</returns>
        /// <exception cref="ErrorException"></exception>
        public Task<DefaultOptionProviderResult> ProvideAsync(DefaultOptionProviderContext context)
        {
            if (context.CommandDescriptor.OptionDescriptors == null || context.CommandDescriptor.DefaultArgument == null)
            {
                throw new ErrorException(Errors.UnsupportedOption, "The command does not support default option. command_id={0} command_name={1}", context.CommandDescriptor.Id, context.CommandDescriptor.Name);
            }

            return Task.FromResult(new DefaultOptionProviderResult(context.CommandDescriptor.OptionDescriptors[context.CommandDescriptor.DefaultArgument]));
        }

        private readonly ILogger<DefaultOptionProvider> logger;
        private readonly CliOptions options;
    }
}
