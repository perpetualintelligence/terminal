/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Infrastructure;

namespace PerpetualIntelligence.Cli.Commands.Providers
{
    /// <summary>
    /// The argument default provider result.
    /// </summary>
    public class DefaultArgumentProviderResult : ResultNoError
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="defaultArgumentDescriptor">The default argument descriptor.</param>
        public DefaultArgumentProviderResult(ArgumentDescriptor defaultArgumentDescriptor)
        {
            DefaultArgumentDescriptor = defaultArgumentDescriptor ?? throw new System.ArgumentNullException(nameof(defaultArgumentDescriptor));
        }

        /// <summary>
        /// The default argument descriptor.
        /// </summary>
        public ArgumentDescriptor DefaultArgumentDescriptor { get; }
    }
}
