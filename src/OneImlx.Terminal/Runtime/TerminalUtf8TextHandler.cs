/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalTextHandler"/> for <see cref="Encoding.UTF8"/> and <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </summary>
    public sealed class TerminalUtf8TextHandler : ITerminalTextHandler
    {
        /// <summary>
        /// The <see cref="StringComparison.InvariantCultureIgnoreCase"/> string comparison.
        /// </summary>
        public StringComparison Comparison => StringComparison.InvariantCultureIgnoreCase;

        /// <summary>
        /// The <see cref="Encoding.UTF8"/> text encoding.
        /// </summary>
        public Encoding Encoding => Encoding.UTF8;

        /// <summary>
        /// Returns the <see cref="StringComparer.InvariantCultureIgnoreCase"/> equality comparer.
        /// </summary>
        public IEqualityComparer<string> EqualityComparer()
        {
            return StringComparer.OrdinalIgnoreCase;
        }

        /// <summary>
        /// Determines whether the two ASCII texts are equal using <see cref="Comparison"/>.
        /// </summary>
        /// <param name="s1">The first text to compare.</param>
        /// <param name="s2">The second text to compare.</param>
        /// <returns><c>true</c> if the texts are equal, <c>false</c> otherwise.</returns>
        public bool TextEquals(string? s1, string? s2)
        {
            return string.Equals(s1, s2, Comparison);
        }

        /// <summary>
        /// Returns the ASCII text length.
        /// </summary>
        public int TextLength(string? s1)
        {
            if (s1 == null)
            {
                return 0;
            }

            return s1.Length;
        }
    }
}
