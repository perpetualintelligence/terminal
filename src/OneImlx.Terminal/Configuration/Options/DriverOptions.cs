/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace OneImlx.Terminal.Configuration.Options
{
    /// <summary>
    /// The driver configuration options. Reserved for future use.
    /// </summary>
    public sealed class DriverOptions
    {
        /// <summary>
        /// Determines if the terminal is a native driver program.
        /// </summary>
        /// <remarks>
        /// If enabled, the terminal's root command is also a native command prompt driver program. You can execute your
        /// commands from a native command prompt by specifying the root and command options.
        /// </remarks>
        public bool? Enabled { get; set; }

        /// <summary>
        /// The terminal driver program name.
        /// </summary>
        public string? Name { get; set; }
    }
}
