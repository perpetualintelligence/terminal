/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
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
        public string ArgumentValueSeparator { get; set; } = "=";

        /// <summary>
        /// The command string separator. Defaults to space char <c></c>.
        /// </summary>
        public string Separator { get; set; } = " ";
    }
}
