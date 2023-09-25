/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    /// <summary>
    /// Represents an parsed segment split based on a token.
    /// </summary>
    public sealed class ParsedSplit
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="split">The split option.</param>
        /// <param name="token">The token used to split.</param>
        public ParsedSplit(string split, string? token)
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