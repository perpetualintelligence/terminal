/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Cli.Commands.Providers
{
    /// <summary>
    /// The option default provider result.
    /// </summary>
    public class DefaultOptionProviderResult
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="defaultArgumentDescriptor">The default option descriptor.</param>
        public DefaultOptionProviderResult(OptionDescriptor defaultArgumentDescriptor)
        {
            DefaultArgumentDescriptor = defaultArgumentDescriptor ?? throw new System.ArgumentNullException(nameof(defaultArgumentDescriptor));
        }

        /// <summary>
        /// The default option descriptor.
        /// </summary>
        public OptionDescriptor DefaultArgumentDescriptor { get; }
    }
}