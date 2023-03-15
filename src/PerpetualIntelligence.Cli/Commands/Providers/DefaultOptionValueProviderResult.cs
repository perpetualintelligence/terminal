/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Infrastructure;

namespace PerpetualIntelligence.Cli.Commands.Providers
{
    /// <summary>
    /// The option default value provider result.
    /// </summary>
    public class DefaultOptionValueProviderResult
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="defaultValueOptionDescriptors">The default value option descriptors.</param>
        public DefaultOptionValueProviderResult(OptionDescriptors defaultValueOptionDescriptors)
        {
            DefaultValueOptionDescriptors = defaultValueOptionDescriptors ?? throw new System.ArgumentNullException(nameof(defaultValueOptionDescriptors));
        }

        /// <summary>
        /// The default value option descriptors.
        /// </summary>
        public OptionDescriptors DefaultValueOptionDescriptors { get; }
    }
}
