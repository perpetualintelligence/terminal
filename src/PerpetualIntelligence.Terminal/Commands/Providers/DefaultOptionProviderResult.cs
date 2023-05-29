/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Commands.Providers
{
    /// <summary>
    /// The option default provider result.
    /// </summary>
    public class DefaultOptionProviderResult
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="defaultOptionDescriptor">The default option descriptor.</param>
        public DefaultOptionProviderResult(OptionDescriptor defaultOptionDescriptor)
        {
            DefaultOptionDescriptor = defaultOptionDescriptor ?? throw new System.ArgumentNullException(nameof(defaultOptionDescriptor));
        }

        /// <summary>
        /// The default option descriptor.
        /// </summary>
        public OptionDescriptor DefaultOptionDescriptor { get; }
    }
}