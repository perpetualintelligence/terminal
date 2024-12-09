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
    /// An abstraction of a terminal text handler.
    /// </summary>
    public interface ITerminalTextHandler
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
        /// Determines whether the two characters are equal using <see cref="Comparison"/>.
        /// </summary>
        /// <param name="ch1">The first char to compare.</param>
        /// <param name="ch2">The second char to compare.</param>
        /// <returns><c>true</c> if the characters are equal, <c>false</c> otherwise.</returns>
        public bool CharEquals(char? ch1, char? ch2);

        /// <summary>
        /// Returns the equality comparer.
        /// </summary>
        public IEqualityComparer<string> EqualityComparer();

        /// <summary>
        /// Determines whether a characters and text are equal using <see cref="Comparison"/>.
        /// </summary>
        /// <param name="ch1">The first char to compare.</param>
        /// <param name="s2">The second text to compare.</param>
        /// <returns><c>true</c> if the texts are equal, <c>false</c> otherwise.</returns>
        public bool SingleEquals(char? ch1, string? s2);

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
