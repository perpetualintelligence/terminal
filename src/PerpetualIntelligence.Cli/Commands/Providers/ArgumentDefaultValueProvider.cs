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
    public class ArgumentDefaultValueProvider : IArgumentDefaultValueProvider
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public ArgumentDefaultValueProvider(CliOptions options, ILogger<ArgumentDefaultValueProvider> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        /// <summary>
        /// Provides default values for all the command arguments.
        /// </summary>
        /// <param name="context">The argument default value provider context.</param>
        /// <returns>The <see cref="ArgumentDefaultValueProviderResult"/> instance that contains the default values.</returns>
        /// <exception cref="ErrorException"></exception>
        public Task<ArgumentDefaultValueProviderResult> ProvideAsync(ArgumentDefaultValueProviderContext context)
        {
            if (context.CommandDescriptor.ArgumentDescriptors == null || context.CommandDescriptor.ArgumentDescriptors.Count == 0)
            {
                throw new ErrorException(Errors.UnsupportedArgument, "The command does not support any arguments. command_id={0} command_name={1}", context.CommandDescriptor.Id, context.CommandDescriptor.Name);
            }

            return Task.FromResult(new ArgumentDefaultValueProviderResult(new ArgumentDescriptors(context.CommandDescriptor.ArgumentDescriptors.Where(a => a.DefaultValue != null))));
        }

        private ILogger<ArgumentDefaultValueProvider> logger;
        private CliOptions options;
    }
}
