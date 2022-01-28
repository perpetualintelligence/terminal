/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The ordered <see cref="ArgumentDescriptor"/> collection.
    /// </summary>
    /// <remarks>
    /// The argument descriptor collection comparer is <see cref="StringComparer.Ordinal"/> and it determines whether
    /// two <see cref="ArgumentDescriptor.Id"/> strings are equal. Every argument descriptor in the collection must have
    /// unique id.
    /// </remarks>
    public sealed class ArgumentDescriptors : KeyedCollection<string, ArgumentDescriptor>
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ArgumentDescriptors() : base(StringComparer.Ordinal)
        {
        }

        /// <summary>
        /// Initializes a new instance with the specified argument descriptors.
        /// </summary>
        /// <param name="collection">The argument descriptors.</param>
        public ArgumentDescriptors(IEnumerable<ArgumentDescriptor> collection) : this()
        {
            foreach (ArgumentDescriptor argumentDescriptor in collection)
            {
                Add(argumentDescriptor);
            }
        }

        /// <inheritdoc/>
        protected override string GetKeyForItem(ArgumentDescriptor item)
        {
            return item.Id;
        }
    }
}
