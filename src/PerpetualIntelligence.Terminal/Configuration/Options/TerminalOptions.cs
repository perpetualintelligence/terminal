/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Configuration.Options
{
    /// <summary>
    /// The terminal configuration options.
    /// </summary>
    public class TerminalOptions
    {
        /// <summary>
        /// The terminal identifier.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The driver configuration options. Reserved for future versions.
        /// </summary>
        public DriverOptions Driver { get; set; } = new DriverOptions();

        /// <summary>
        /// The authentication configuration options. Reserved for future versions.
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
        /// The handler configuration options.
        /// </summary>
        public HandlerOptions Handler { get; set; } = new HandlerOptions();

        /// <summary>
        /// The HTTP configuration options.
        /// </summary>
        public HttpOptions Http { get; set; } = new HttpOptions();

        /// <summary>
        /// The licensing configuration options.
        /// </summary>
        public LicensingOptions Licensing { get; set; } = new LicensingOptions();

        /// <summary>
        /// The router configuration options.
        /// </summary>
        public RouterOptions Router { get; set; } = new RouterOptions();

        /// <summary>
        /// The help configuration options.
        /// </summary>
        public HelpOptions Help { get; set; } = new HelpOptions();
    }
}