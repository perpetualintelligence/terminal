/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PerpetualIntelligence.Cli.Commands.Handlers
{
    /// <summary>
    /// The default <see cref="ITextHandler"/> for <see cref="Encoding.Unicode"/> and <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </summary>
    public class UnicodeTextHandler : ITextHandler
    {
        /// <summary>
        /// The <see cref="StringComparison.OrdinalIgnoreCase"/> string comparison.
        /// </summary>
        public StringComparison Comparison => StringComparison.OrdinalIgnoreCase;

        /// <summary>
        /// The Unicode text encoding.
        /// </summary>
        public Encoding Encoding => Encoding.Unicode;

        /// <summary>
        /// Returns the <see cref="StringComparer.OrdinalIgnoreCase"/> equality comparer.
        /// </summary>
        public IEqualityComparer<string> EqualityComparer()
        {
            return StringComparer.OrdinalIgnoreCase;
        }

        /// <summary>
        /// Determines whether the two Unicode texts are equal using <see cref="Comparison"/>.
        /// </summary>
        /// <param name="s1">The first text to compare.</param>
        /// <param name="s2">The second text to compare.</param>
        /// <returns><c>true</c> if the texts are equal, <c>false</c> otherwise.</returns>
        public bool TextEquals(string? s1, string? s2)
        {
            return string.Equals(s1, s2, Comparison);
        }

        /// <summary>
        /// Returns the Unicode text length.
        /// </summary>
        public int TextLength(string? s1)
        {
            if (s1 == null)
            {
                return 0;
            }

            StringInfo sinfo = new(s1);
            return sinfo.LengthInTextElements;
        }
    }
}
