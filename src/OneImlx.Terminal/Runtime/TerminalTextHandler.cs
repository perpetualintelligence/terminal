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
    /// The default <see cref="ITerminalTextHandler"/>.
    /// </summary>
    public sealed class TerminalTextHandler : ITerminalTextHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalTextHandler"/> class with specified comparison and encoding.
        /// </summary>
        /// <param name="comparison">The string comparison to use.</param>
        /// <param name="encoding">The text encoding to use.</param>
        public TerminalTextHandler(StringComparison comparison, Encoding encoding)
        {
            Comparison = comparison;
            Encoding = encoding;
        }

        /// <summary>
        /// The string comparison.
        /// </summary>
        public StringComparison Comparison { get; }

        /// <summary>
        /// The text encoding.
        /// </summary>
        public Encoding Encoding { get; }

        /// <inheritdoc/>
        public bool CharEquals(char? ch1, char? ch2)
        {
            return char.Equals(ch1, ch2);
        }

        /// <summary>
        /// Returns the <see cref="StringComparer.InvariantCultureIgnoreCase"/> equality comparer.
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
