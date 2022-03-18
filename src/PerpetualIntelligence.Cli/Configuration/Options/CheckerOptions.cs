/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
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
        /// Determines whether the checker allows strict type checking.
        /// </summary>
        public bool? AllowStrictTypeChecking { get; set; }
    }
}
