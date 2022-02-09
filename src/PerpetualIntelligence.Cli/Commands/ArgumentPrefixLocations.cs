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
    /// The ordered keyed collection of the argument prefix location.
    /// </summary>
    /// <remarks>
    /// The argument collection comparer is <see cref="StringComparer.Ordinal"/> and it determines whether two
    /// <see cref="Argument.Id"/> strings are equal. Every argument in the collection must have unique id.
    /// </remarks>
    public sealed class ArgumentPrefixLocations : KeyedCollection<int, ArgumentPrefixLocation>
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ArgumentPrefixLocations() : base()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override int GetKeyForItem(ArgumentPrefixLocation item)
        {
            return item.Location;
        }
    }
}
