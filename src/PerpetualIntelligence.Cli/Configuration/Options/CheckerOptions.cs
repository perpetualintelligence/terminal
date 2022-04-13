/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The checker configuration option.
    /// </summary>
    public class CheckerOptions
    {
        /// <summary>
        /// Determines whether the checker allows an obsolete argument.
        /// </summary>
        public bool? AllowObsoleteArgument { get; set; }

        /// <summary>
        /// Defines the checker data type checks. Defaults to <c>null</c> or no data type checks.
        /// </summary>
        public string? DataTypeCheck { get; set; }

        /// <summary>
        /// Determines whether the checker allows strict type checking.
        /// </summary>
        public bool? StrictTypeChecking { get; set; }
    }
}
