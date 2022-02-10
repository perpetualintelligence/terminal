/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Collections.ObjectModel;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The ordered <see cref="ArgumentString"/> collection.
    /// </summary>
    /// <remarks>
    /// The <see cref="ArgumentStrings"/> collection determines the <see cref="ArgumentString.Position"/> of each
    /// <see cref="ArgumentString"/> with in a <see cref="CommandString"/>.
    /// </remarks>
    public sealed class ArgumentStrings : KeyedCollection<int, ArgumentString>
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ArgumentStrings() : base()
        {
        }

        /// <summary>
        /// Gets the <see cref="ArgumentString.Position"/> as a key for the item.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>The <see cref="ArgumentString.Position"/> as a key for the item.</returns>
        protected override int GetKeyForItem(ArgumentString item)
        {
            return item.Position;
        }
    }
}
