/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace OneImlx.Terminal.Commands.Handlers
{
    /// <summary>
    /// An abstraction of text handler.
    /// </summary>
    public interface ITextHandler
    {
        /// <summary>
        /// Gets the <see cref="StringComparison"/> value to compare the two strings.
        /// </summary>
        public StringComparison Comparison { get; }

        /// <summary>
        /// Gets the text <see cref="Encoding"/>.
        /// </summary>
        public Encoding Encoding { get; }

        /// <summary>
        /// Returns the equality comparer.
        /// </summary>
        public IEqualityComparer<string> EqualityComparer();

        /// <summary>
        /// Determines whether the two texts are equal using <see cref="Comparison"/>.
        /// </summary>
        /// <param name="s1">The first text to compare.</param>
        /// <param name="s2">The second text to compare.</param>
        /// <returns><c>true</c> if the texts are equal, <c>false</c> otherwise.</returns>
        public bool TextEquals(string? s1, string? s2);

        /// <summary>
        /// Returns the text length.
        /// </summary>
        public int TextLength(string? s1);
    }
}