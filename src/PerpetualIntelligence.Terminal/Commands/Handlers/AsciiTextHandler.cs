/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace PerpetualIntelligence.Terminal.Commands.Handlers
{
    /// <summary>
    /// The default <see cref="ITextHandler"/> for <see cref="Encoding.ASCII"/> and <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </summary>
    public sealed class AsciiTextHandler : ITextHandler
    {
        /// <summary>
        /// The <see cref="StringComparison.InvariantCultureIgnoreCase"/> string comparison.
        /// </summary>
        public StringComparison Comparison => StringComparison.InvariantCultureIgnoreCase;

        /// <summary>
        /// The ASCII text encoding.
        /// </summary>
        public Encoding Encoding => Encoding.ASCII;

        /// <summary>
        /// Returns the <see cref="StringComparer.InvariantCultureIgnoreCase"/> equality comparer.
        /// </summary>
        public IEqualityComparer<string> EqualityComparer()
        {
            return StringComparer.OrdinalIgnoreCase;
        }

        /// <summary>
        /// Returns the default extraction regex pattern for ASCII command string.
        /// </summary>
        public string ExtractionRegex()
        {
            return @"^(\w+)\s+((?:\w+\s+)*)(\w+)((?:\s+-{1,2}\w+(?:\s+(?:'[^']*'|[^\s']+(?=\s+|$)))*)+)$";
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