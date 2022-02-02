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
        /// The argument prefix. Defaults to dash char <c>-</c>.
        /// </summary>
        public string? ArgumentPrefix { get; set; } = "-";

        /// <summary>
        /// The argument value separator. Defaults to equals char <c>=</c>.
        /// </summary>
        public string ArgumentSeparator { get; set; } = "=";

        /// <summary>
        /// Determines whether the extractor support extracting default arguments. Defaults to <c>false</c>.
        /// </summary>
        public bool? DefaultArgument { get; set; } = false;

        /// <summary>
        /// Determines whether the extractor support extracting default argument values. Defaults to <c>false</c>.
        /// </summary>
        public bool? DefaulValue { get; set; } = false;

        /// <summary>
        /// The command string separator. Defaults to a single space char.
        /// </summary>
        public string Separator { get; set; } = " ";

        /// <summary>
        /// Defines the token within which to extract a string value. Default to <c>null</c>.
        /// </summary>
        public string? StringWithIn = null;
    }
}
