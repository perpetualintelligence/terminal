/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Collections.ObjectModel;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The ordered <see cref="OptionString"/> collection.
    /// </summary>
    /// <remarks>
    /// The <see cref="OptionStrings"/> collection determines the <see cref="OptionString.Position"/> of each
    /// <see cref="OptionString"/> with in a <see cref="CommandString"/>.
    /// </remarks>
    public sealed class OptionStrings : KeyedCollection<int, OptionString>
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public OptionStrings() : base()
        {
        }

        /// <summary>
        /// Gets the <see cref="OptionString.Position"/> as a key for the item.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>The <see cref="OptionString.Position"/> as a key for the item.</returns>
        protected override int GetKeyForItem(OptionString item)
        {
            return item.Position;
        }
    }
}
