/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The <c>cli</c> configuration options.
    /// </summary>
    public class CliOptions
    {
        /// <summary>
        /// The authentication configuration options.
        /// </summary>
        public AuthenticationOptions Authentication { get; set; } = new AuthenticationOptions();

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
        /// The HTTP configuration options.
        /// </summary>
        public HttpOptions Http { get; set; } = new HttpOptions();

        /// <summary>
        /// The licensing configuration options.
        /// </summary>
        public LicensingOptions Licensing { get; set; } = new LicensingOptions();

        /// <summary>
        /// The logging configuration options.
        /// </summary>
        public LoggingOptions Logging { get; set; } = new LoggingOptions();
    }
}
