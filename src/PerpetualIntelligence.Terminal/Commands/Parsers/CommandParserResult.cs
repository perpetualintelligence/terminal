/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Commands.Parsers
{
    /// <summary>
    /// The command parser result.
    /// </summary>
    public sealed class CommandParserResult
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="parsedCommand">The parsed command.</param>
        public CommandParserResult(ParsedCommand parsedCommand)
        {
            ParsedCommand = parsedCommand ?? throw new System.ArgumentNullException(nameof(parsedCommand));
        }

        /// <summary>
        /// The parsed command.
        /// </summary>
        public ParsedCommand ParsedCommand { get; }
    }
}