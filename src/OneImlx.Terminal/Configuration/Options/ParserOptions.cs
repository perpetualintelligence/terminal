/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace OneImlx.Terminal.Configuration.Options
{
    /// <summary>
    /// The terminal parser options.
    /// </summary>
    public sealed class ParserOptions
    {
        /// <summary>
        /// The option alias prefix. Defaults to <c>-</c>.
        /// </summary>
        /// <remarks>The option alias prefix cannot be <c>null</c> or whitespace.</remarks>
        public string OptionAliasPrefix { get; set; } = "-";

        /// <summary>
        /// The option prefix. Defaults to <c>--</c>.
        /// </summary>
        /// <remarks>The option prefix cannot be <c>null</c> or whitespace.</remarks>
        public string OptionPrefix { get; set; } = "--";

        /// <summary>
        /// The option value separator. Defaults to <c></c>.
        /// </summary>
        /// <remarks>The option value separator must be a single Unicode character, and it can be a single whitespace.</remarks>
        public char OptionValueSeparator { get; set; } = TerminalIdentifiers.SpaceSeparator;

        /// <summary>
        /// The temporary runtime separator that is used to optimize the parsing algorithm. Defaults to <c>0x1F</c> or
        /// Unit Separator.
        /// </summary>
        public char RuntimeSeparator { get; set; } = '\u001F';

        /// <summary>
        /// The command string separator. Defaults to a single whitespace.
        /// </summary>
        /// <remarks>
        /// The command string separator must be a single Unicode character, and it can be a whitespace character.
        /// </remarks>
        public char Separator { get; set; } = TerminalIdentifiers.SpaceSeparator;

        /// <summary>
        /// The value delimiter used to extract a value within the configured delimiter. Defaults to <c>"</c>.
        /// </summary>
        public char ValueDelimiter { get; set; } = '"';
    }
}
