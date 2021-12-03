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
        /// The argument key value prefix. Set it to <c>null</c> if you don't want to use argument prefix.
        /// </summary>
        public char? KeyValuePrefix { get; set; } = '-';

        /// <summary>
        /// The argument key value separator. Defaults to equals char <c>=</c>.
        /// </summary>
        public char KeyValueSeparator { get; set; } = '=';

        /// <summary>
        /// The command string separator. Defaults to space char <c></c>.
        /// </summary>
        public char Separator { get; set; } = ' ';
    }
}
