/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace OneImlx.Terminal.Configuration.Options
{
    /// <summary>
    /// The terminal configuration options.
    /// </summary>
    public sealed class TerminalOptions
    {
        /// <summary>
        /// The authentication configuration options. Reserved for future versions.
        /// </summary>
        public AuthenticationOptions Authentication { get; set; } = new AuthenticationOptions();

        /// <summary>
        /// The checker configuration options.
        /// </summary>
        public CheckerOptions Checker { get; set; } = new CheckerOptions();

        /// <summary>
        /// The driver configuration options. Reserved for future versions.
        /// </summary>
        public DriverOptions Driver { get; set; } = new DriverOptions();

        /// <summary>
        /// The help configuration options.
        /// </summary>
        public HelpOptions Help { get; set; } = new HelpOptions();

        /// <summary>
        /// The terminal identifier.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The integration configuration options.
        /// </summary>
        public IntegrationOptions Integration { get; set; } = new IntegrationOptions();

        /// <summary>
        /// The licensing configuration options.
        /// </summary>
        public LicensingOptions Licensing { get; set; } = new LicensingOptions();

        /// <summary>
        /// The parser configuration options.
        /// </summary>
        public ParserOptions Parser { get; set; } = new ParserOptions();

        /// <summary>
        /// The router configuration options.
        /// </summary>
        public RouterOptions Router { get; set; } = new RouterOptions();
    }
}
