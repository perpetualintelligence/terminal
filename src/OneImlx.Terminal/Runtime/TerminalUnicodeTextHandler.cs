/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalTextHandler"/> for <see cref="Encoding.Unicode"/> and <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </summary>
    public sealed class TerminalUnicodeTextHandler : ITerminalTextHandler
    {
        /// <summary>
        /// The <see cref="StringComparison.OrdinalIgnoreCase"/> string comparison.
        /// </summary>
        public StringComparison Comparison => StringComparison.OrdinalIgnoreCase;

        /// <summary>
        /// The Unicode text encoding.
        /// </summary>
        public Encoding Encoding => Encoding.Unicode;

        /// <inheritdoc/>
        public bool CharEquals(char? ch1, char? ch2)
        {
            return char.Equals(ch1, ch2);
        }

        /// <summary>
        /// Returns the <see cref="StringComparer.OrdinalIgnoreCase"/> equality comparer.
        /// </summary>
        public IEqualityComparer<string> EqualityComparer()
        {
            return StringComparer.OrdinalIgnoreCase;
        }

        /// <inheritdoc/>
        public bool SingleEquals(char? ch1, string? s2)
        {
            if (ch1 == null || s2 == null)
            {
                return false;
            }

            if (s2.Length != 1)
            {
                return false;
            }

            return TextEquals(ch1.ToString(), s2);
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
