﻿/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Infrastructure;

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The <c>cli</c> configuration options.
    /// </summary>
    public class CliOptions
    {
        /// <summary>
        /// The extractor configuration options.
        /// </summary>
        public ExtractorOptions Extractor { get; set; } = new ExtractorOptions();

        /// <summary>
        /// The logging configuration options.
        /// </summary>
        public OneImlxLoggingOptions Logging { get; set; } = new OneImlxLoggingOptions();
    }
}
