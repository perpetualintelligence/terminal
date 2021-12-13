/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The <c>cli</c> configuration options.
    /// </summary>
    public class CliOptions
    {
        /// <summary>
        /// The checker configuration options.
        /// </summary>
        public CheckerOptions Checker { get; set; } = new CheckerOptions();

        /// <summary>
        /// The extractor configuration options.
        /// </summary>
        public ExtractorOptions Extractor { get; set; } = new ExtractorOptions();

        /// <summary>
        /// The hosting configuration options.
        /// </summary>
        public HostingOptions Hosting { get; set; } = new HostingOptions();

        /// <summary>
        /// The logging configuration options.
        /// </summary>
        public LoggingOptions Logging { get; set; } = new LoggingOptions();
    }
}
