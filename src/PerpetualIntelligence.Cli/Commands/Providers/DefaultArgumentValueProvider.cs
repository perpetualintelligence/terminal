/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Shared.Exceptions;
using System.Linq;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Providers
{
    /// <summary>
    /// The default argument default value provider.
    /// </summary>
    public class DefaultArgumentValueProvider : IDefaultArgumentValueProvider
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public DefaultArgumentValueProvider(CliOptions options, ILogger<DefaultArgumentValueProvider> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        /// <summary>
        /// Provides default values for all the command arguments.
        /// </summary>
        /// <param name="context">The argument default value provider context.</param>
        /// <returns>The <see cref="DefaultArgumentValueProviderResult"/> instance that contains the default values.</returns>
        /// <exception cref="ErrorException"></exception>
        public Task<DefaultArgumentValueProviderResult> ProvideAsync(DefaultArgumentValueProviderContext context)
        {
            if (context.CommandDescriptor.ArgumentDescriptors == null || context.CommandDescriptor.ArgumentDescriptors.Count == 0)
            {
                throw new ErrorException(Errors.UnsupportedArgument, "The command does not support any arguments. command_id={0} command_name={1}", context.CommandDescriptor.Id, context.CommandDescriptor.Name);
            }

            return Task.FromResult(new DefaultArgumentValueProviderResult(new ArgumentDescriptors(context.CommandDescriptor.ArgumentDescriptors.Where(a => a.DefaultValue != null))));
        }

        private ILogger<DefaultArgumentValueProvider> logger;
        private CliOptions options;
    }
}
