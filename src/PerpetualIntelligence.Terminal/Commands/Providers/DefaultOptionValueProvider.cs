/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Handlers;

using PerpetualIntelligence.Shared.Exceptions;
using System.Linq;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Providers
{
    /// <summary>
    /// The default option default value provider.
    /// </summary>
    public class DefaultOptionValueProvider : IDefaultOptionValueProvider
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="textHandler">The text handler.</param>
        public DefaultOptionValueProvider(ITextHandler textHandler)
        {
            this.textHandler = textHandler;
        }

        /// <summary>
        /// Provides default values for all the command options.
        /// </summary>
        /// <param name="context">The option default value provider context.</param>
        /// <returns>The <see cref="DefaultOptionValueProviderResult"/> instance that contains the default values.</returns>
        /// <exception cref="ErrorException"></exception>
        public Task<DefaultOptionValueProviderResult> ProvideAsync(DefaultOptionValueProviderContext context)
        {
            if (context.CommandDescriptor.OptionDescriptors == null || context.CommandDescriptor.OptionDescriptors.Count == 0)
            {
                throw new ErrorException(Errors.UnsupportedOption, "The command does not support any options. command_id={0} command_name={1}", context.CommandDescriptor.Id, context.CommandDescriptor.Name);
            }

            return Task.FromResult(new DefaultOptionValueProviderResult(new OptionDescriptors(textHandler, context.CommandDescriptor.OptionDescriptors.Where(a => a.DefaultValue != null))));
        }

        private readonly ITextHandler textHandler;
    }
}
