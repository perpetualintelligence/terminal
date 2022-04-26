/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The <c>pi-cli</c> command and argument extraction options.
    /// </summary>
    public class ExtractorOptions
    {
        /// <summary>
        /// Determines whether the extractor support extracting an argument by alias.
        /// </summary>
        /// <remarks>
        /// Argument alias supports the apps that identify a command argument with an id and an alias string. For modern
        /// console apps, we recommend using just an argument identifier. We have optimized the core data model to work
        /// with argument id. An app should not identify the same argument with multiple strings. Using an alias will
        /// degrade the performance.
        /// </remarks>
        public bool? ArgumentAlias { get; set; }

        /// <summary>
        /// The argument alias prefix if <see cref="ArgumentAlias"/> is enabled. Defaults to <c>-</c>.
        /// </summary>
        /// <remarks>The argument alias prefix must be a single Unicode character, and it cannot be <c>null</c> or whitespace.</remarks>
        public string ArgumentAliasPrefix { get; set; } = "-";

        /// <summary>
        /// The argument prefix. Defaults to <c>-</c>.
        /// </summary>
        /// <remarks>The argument prefix must be a single Unicode character, and it cannot be <c>null</c> or whitespace.</remarks>
        public string ArgumentPrefix { get; set; } = "-";

        /// <summary>
        /// The argument value separator. Defaults to <c>=</c>.
        /// </summary>
        /// <remarks>The argument value separator must be a single Unicode character, and it can be a single whitespace.</remarks>
        public string ArgumentValueSeparator { get; set; } = "=";

        /// <summary>
        /// An optional token within which to extract an argument value. Default to <c>null</c>.
        /// </summary>
        /// <remarks>
        /// The optional argument within token must be a single Unicode character. If set it cannot be <c>null</c> or whitespace.
        /// </remarks>
        public string? ArgumentValueWithIn { get; set; }

        /// <summary>
        /// The Regex pattern for command identifier. Defaults to <c>^[A-Za-z0-9_-]*$</c>.
        /// </summary>
        public string CommandIdRegex { get; set; } = "^[A-Za-z0-9_-]*$";

        /// <summary>
        /// Determines whether command supports default argument.
        /// </summary>
        public bool? DefaultArgument { get; set; }

        /// <summary>
        /// Determines whether argument support default value.
        /// </summary>
        public bool? DefaultArgumentValue { get; set; }

        /// <summary>
        /// The command string separator. Defaults to a single whitespace.
        /// </summary>
        /// <remarks>
        /// The command string separator must be a single Unicode character, and it can be a whitespace character.
        /// </remarks>
        public string Separator { get; set; } = " ";
    }
}
