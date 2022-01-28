/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;
using System.Collections.ObjectModel;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The ordered <see cref="Argument"/> keyed collection.
    /// </summary>
    /// <remarks>
    /// The argument collection comparer is <see cref="StringComparer.Ordinal"/> and it determines whether two
    /// <see cref="Argument.Id"/> strings are equal. Every argument in the collection must have unique id.
    /// </remarks>
    public sealed class Arguments : KeyedCollection<string, Argument>
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public Arguments() : base(StringComparer.Ordinal)
        {
        }

        /// <summary>
        /// Gets the argument value.
        /// </summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        public TValue GetValue<TValue>(string argumentId)
        {
            return (TValue)this[argumentId].Value;
        }

        /// <inheritdoc/>
        protected override string GetKeyForItem(Argument item)
        {
            return item.Id;
        }
    }
}
