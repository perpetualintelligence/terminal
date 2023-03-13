/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Infrastructure;

namespace PerpetualIntelligence.Cli.Commands.Providers
{
    /// <summary>
    /// The argument default value provider result.
    /// </summary>
    public class DefaultArgumentValueProviderResult
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="defaultValueArgumentDescriptors">The default value argument descriptors.</param>
        public DefaultArgumentValueProviderResult(OptionDescriptors defaultValueArgumentDescriptors)
        {
            DefaultValueArgumentDescriptors = defaultValueArgumentDescriptors ?? throw new System.ArgumentNullException(nameof(defaultValueArgumentDescriptors));
        }

        /// <summary>
        /// The default value argument descriptors.
        /// </summary>
        public OptionDescriptors DefaultValueArgumentDescriptors { get; }
    }
}
