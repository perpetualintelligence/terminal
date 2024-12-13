/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace OneImlx.Terminal.Configuration.Options
{
    /// <summary>
    /// Represents the terminal parser options.
    /// </summary>
    public sealed class ParserOptions
    {
        /// <summary>
        /// Indicates whether option aliasing is disabled. Defaults to <c>false</c>.
        /// </summary>
        /// <remarks>
        /// When option aliasing is disabled, the parser will resolve options using only the <see cref="OptionPrefix"/>.
        /// When option aliasing is enabled, the parser will recognize and resolve options using:
        /// <list type="bullet">
        /// <item>
        /// <term>Short Name</term>
        /// <description>Short option or alias prefixed by <c>one</c><see cref="OptionPrefix"/> (e.g., <c>-a</c>).</description>
        /// </item>
        /// <item>
        /// <term>Long Name</term>
        /// <description>Long option prefixed by <c>two</c><see cref="OptionPrefix"/> (e.g., <c>--all</c>).</description>
        /// </item>
        /// <item>
        /// <term>Hybrid Name</term>
        /// <description>Hybrid option with both short and long option naming (e.g., <c>--long-name</c>).</description>
        /// </item>
        /// </list>
        /// </remarks>
        public bool DisableOptionAlias { get; set; } = false;

        /// <summary>
        /// Specifies the prefix used for options. Defaults to <c>-</c>.
        /// </summary>
        /// <remarks>The <see cref="OptionPrefix"/> is required and cannot be <c>null</c> or consist only of whitespace.</remarks>
        public char OptionPrefix { get; set; } = '-';

        /// <summary>
        /// Gets or sets the option value separator. Defaults to <c></c>.
        /// </summary>
        /// <remarks>The option value separator must be a single Unicode character, and it can be a single whitespace.</remarks>
        public char OptionValueSeparator { get; set; } = TerminalIdentifiers.SpaceSeparator;

        /// <summary>
        /// Gets or sets the temporary runtime separator that is used to optimize the parsing algorithm. Defaults to
        /// <c>0x1F</c> or Unit Separator.
        /// </summary>
        public char RuntimeSeparator { get; set; } = '\u001F';

        /// <summary>
        /// Gets or sets the command string separator. Defaults to a single whitespace.
        /// </summary>
        /// <remarks>
        /// The command string separator must be a single Unicode character, and it can be a whitespace character.
        /// </remarks>
        public char Separator { get; set; } = TerminalIdentifiers.SpaceSeparator;

        /// <summary>
        /// Gets or sets the value delimiter used to extract a value within the configured delimiter. Defaults to <c>"</c>.
        /// </summary>
        public char ValueDelimiter { get; set; } = '"';
    }
}
