/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
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
        /// <param name="defaultValueArgumentDescriptors">The default value option descriptors.</param>
        public DefaultOptionValueProviderResult(OptionDescriptors defaultValueArgumentDescriptors)
        {
            DefaultValueArgumentDescriptors = defaultValueArgumentDescriptors ?? throw new System.ArgumentNullException(nameof(defaultValueArgumentDescriptors));
        }

        /// <summary>
        /// The default value option descriptors.
        /// </summary>
        public OptionDescriptors DefaultValueArgumentDescriptors { get; }
    }
}
