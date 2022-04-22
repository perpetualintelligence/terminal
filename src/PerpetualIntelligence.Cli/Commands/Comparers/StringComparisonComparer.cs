/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Abstractions.Comparers;
using System;

namespace PerpetualIntelligence.Cli.Commands.Comparers
{
    /// <summary>
    /// The default <see cref="StringComparison"/> based <see cref="IStringComparer"/> to compare the <c>pi-cli</c> command
    /// strings, argument strings and argument values.
    /// </summary>
    public class StringComparisonComparer : IStringComparer
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="comparison"></param>
        public StringComparisonComparer(StringComparison comparison)
        {
            Comparison = comparison;
        }

        /// <summary>
        /// Gets the <see cref="StringComparison"/> value to compare the two strings.
        /// </summary>
        public StringComparison Comparison { get; }

        /// <summary>
        /// Creates a new instance with specified <see cref="StringComparison"/>.
        /// </summary>
        /// <param name="comparison">The <see cref="StringComparison"/> to use.</param>
        /// <returns>A new <see cref="StringComparisonComparer"/> instance.</returns>
        /// <remarks>
        /// <see cref="New(StringComparison)"/> method is useful to create a new instance where dependency injection is
        /// not available to inject the registered <see cref="IStringComparer"/>.
        /// </remarks>
        public static IStringComparer New(StringComparison comparison)
        {
            return new StringComparisonComparer(comparison);
        }

        /// <summary>
        /// Determines whether the two <see cref="string"/> objects are equal.
        /// </summary>
        /// <param name="x">The first string to compare.</param>
        /// <param name="y">The second string to compare.</param>
        /// <returns><c>true</c> if the two strings are equal; otherwise, <c>false</c>.</returns>
        public bool Equals(string? x, string? y)
        {
            return string.Equals(x, y, Comparison);
        }

        /// <summary>
        /// Returns a hash code for the string.
        /// </summary>
        /// <param name="obj">The string.</param>
        /// <returns>The hash code.</returns>
        public int GetHashCode(string? obj)
        {
            if (obj == null)
            {
                return 0;
            }

            return obj.GetHashCode();
        }
    }
}
