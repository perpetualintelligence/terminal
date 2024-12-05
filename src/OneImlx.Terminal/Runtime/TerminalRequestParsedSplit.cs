/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// Represents an parsed segment split based on a token.
    /// </summary>
    public sealed class TerminalRequestParsedSplit
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="split">The split option.</param>
        /// <param name="token">The token used to split.</param>
        public TerminalRequestParsedSplit(string split, string? token)
        {
            Split = split;
            Token = token;
        }

        /// <summary>
        /// The parsed split segment.
        /// </summary>
        public string Split { get; }

        /// <summary>
        /// The token used to split.
        /// </summary>
        public string? Token { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Token != null ? $"{Split} - {Token}" : Split;
        }
    }
}
