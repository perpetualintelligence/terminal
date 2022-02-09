/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The extractor configuration option.
    /// </summary>
    public class ExtractorOptions
    {
        /// <summary>
        /// Determines whether the extractor support extracting the argument by alias. Defaults to <c>false</c>.
        /// </summary>
        /// <remarks>
        /// Argument alias supports the legacy apps that identified a command argument with an id and an alias string.
        /// For modern console apps, we recommend using just an argument identifier. The core data model is optimized to
        /// work with argument id. In general, an app should not identify the same argument with multiple string. Using
        /// alias will degrade the performance.
        /// </remarks>
        public bool? ArgumentAlias { get; set; } = false;

        /// <summary>
        /// The argument alias prefix. Defaults to <c>-</c>.
        /// </summary>
        /// <remarks>The argument alias prefix cannot be <c>null</c> or whitespace.</remarks>
        public string ArgumentAliasPrefix { get; set; } = "-";

        /// <summary>
        /// The argument prefix. Defaults to <c>-</c>.
        /// </summary>
        /// <remarks>The argument prefix cannot be <c>null</c> or whitespace.</remarks>
        public string ArgumentPrefix { get; set; } = "-";

        /// <summary>
        /// The argument value separator. Defaults to equals char <c>=</c>.
        /// </summary>
        public string ArgumentSeparator { get; set; } = "=";

        /// <summary>
        /// Determines whether the extractor support extracting default argument values. Defaults to <c>false</c>.
        /// </summary>
        public bool? DefaulArgumentValue { get; set; } = false;

        /// <summary>
        /// Determines whether the extractor support extracting default arguments. Defaults to <c>false</c>.
        /// </summary>
        public bool? DefaultArgument { get; set; } = false;

        /// <summary>
        /// The command string separator. Defaults to a single space char.
        /// </summary>
        public string Separator { get; set; } = " ";

        /// <summary>
        /// Defines the token within which to extract an argument value. Default to <c>null</c>.
        /// </summary>
        public string? ArgumentValueWithIn = null;
    }
}
