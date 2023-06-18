﻿/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Configuration.Options
{
    /// <summary>
    /// The <c>pi-cli</c> configuration options.
    /// </summary>
    public class TerminalOptions
    {
        /// <summary>
        /// Determines if this terminal's root command is also a native command prompt driver program. If <c>true</c>, you can execute your commands from a native command prompt by specifying the root and command options.
        /// </summary>
        public bool RootAsDriver { get; set; } = false;

        /// <summary>
        /// The terminal name.
        /// </summary>
        public string? Name { get; set; }

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
        /// The logging configuration options.
        /// </summary>
        public LoggingOptions Logging { get; set; } = new LoggingOptions();

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