/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The extractor configuration option.
    /// </summary>
    public class ExtractorOptions
    {
        /// <summary>
        /// The argument prefix. Defaults to dash char <c>-</c>. You can set it to <c>null</c> for no prefix.
        /// </summary>
        public string? ArgumentPrefix { get; set; } = "-";

        /// <summary>
        /// The argument value separator. Defaults to equals char <c>=</c>.
        /// </summary>
        public string ArgumentSeparator { get; set; } = "=";

        /// <summary>
        /// The command string separator. Defaults to space char <c></c>.
        /// </summary>
        public string Separator { get; set; } = " ";
    }
}
