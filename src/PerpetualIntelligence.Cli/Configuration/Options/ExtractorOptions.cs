/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The <c>pi-cli</c> command and option extraction options.
    /// </summary>
    public class ExtractorOptions
    {
        /// <summary>
        /// Determines whether the extractor support extracting an option by alias.
        /// </summary>
        /// <remarks>
        /// Option alias supports the apps that identify a command option with an id and an alias string. For modern
        /// console apps, we recommend using just an option identifier. We have optimized the core data model to work
        /// with option id. An app should not identify the same option with multiple strings. Using an alias will
        /// degrade the performance.
        /// </remarks>
        public bool? OptionAlias { get; set; }

        /// <summary>
        /// The option alias prefix if <see cref="OptionAlias"/> is enabled. Defaults to <c>-</c>.
        /// </summary>
        /// <remarks>The option alias prefix must be a single Unicode character, and it cannot be <c>null</c> or whitespace.</remarks>
        public string OptionAliasPrefix { get; set; } = "-";

        /// <summary>
        /// The option prefix. Defaults to <c>-</c>.
        /// </summary>
        /// <remarks>The option prefix must be a single Unicode character, and it cannot be <c>null</c> or whitespace.</remarks>
        public string OptionPrefix { get; set; } = "-";

        /// <summary>
        /// The option value separator. Defaults to <c>=</c>.
        /// </summary>
        /// <remarks>The option value separator must be a single Unicode character, and it can be a single whitespace.</remarks>
        public string OptionValueSeparator { get; set; } = "=";

        /// <summary>
        /// An optional token within which to extract an option value. Default to <c>null</c>.
        /// </summary>
        /// <remarks>
        /// The optional option within token must be a single Unicode character. If set it cannot be <c>null</c> or whitespace.
        /// </remarks>
        public string? OptionValueWithIn { get; set; }

        /// <summary>
        /// The Regex pattern for command identifier. Defaults to <c>^[A-Za-z0-9_-]*$</c>.
        /// </summary>
        public string CommandIdRegex { get; set; } = "^[A-Za-z0-9_-]*$";

        /// <summary>
        /// Determines whether command supports default option.
        /// </summary>
        public bool? DefaultOption { get; set; }

        /// <summary>
        /// Determines whether option support default value.
        /// </summary>
        public bool? DefaultOptionValue { get; set; }

        /// <summary>
        /// The command string separator. Defaults to a single whitespace.
        /// </summary>
        /// <remarks>
        /// The command string separator must be a single Unicode character, and it can be a whitespace character.
        /// </remarks>
        public string Separator { get; set; } = " ";
    }
}