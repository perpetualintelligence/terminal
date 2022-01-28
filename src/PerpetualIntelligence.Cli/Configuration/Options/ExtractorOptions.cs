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
        /// Determines whether the extractor support extracting default argument value. Defaults to <c>false</c>.
        /// </summary>
        public bool? ArgumentDefaultValue { get; set; } = false;

        /// <summary>
        /// The argument prefix. Defaults to dash char <c>-</c>.
        /// </summary>
        public string? ArgumentPrefix { get; set; } = "-";

        /// <summary>
        /// The argument value separator. Defaults to equals char <c>=</c>.
        /// </summary>
        public string ArgumentSeparator { get; set; } = "=";

        /// <summary>
        /// The command string separator. Defaults to a single space char.
        /// </summary>
        public string Separator { get; set; } = " ";
    }
}
