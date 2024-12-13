/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The raw representation of a parsed <see cref="TerminalRequest"/>.
    /// </summary>
    public sealed class TerminalParsedRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalParsedRequest"/> class.
        /// </summary>
        /// <param name="tokens">The parsed tokens that represent a root, groups, command, and arguments.</param>
        /// <param name="options">The parsed options.</param>
        public TerminalParsedRequest(IEnumerable<string> tokens, Dictionary<string, ValueTuple<string, bool>> options)
        {
            Options = options;
            Tokens = tokens;
        }

        /// <summary>
        /// Gets the parsed options.
        /// </summary>
        public Dictionary<string, ValueTuple<string, bool>> Options { get; }

        /// <summary>
        /// Gets the parsed tokens that represent an ordered collection of root, groups, command, and arguments.
        /// </summary>
        public IEnumerable<string> Tokens { get; }
    }
}
