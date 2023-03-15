/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The logging configuration options.
    /// </summary>
    public class LoggingOptions
    {
        /// <summary>
        /// The string used to obscure error description options. The default value is <c>****</c>.
        /// </summary>
        /// <seealso cref="ObsureInvalidOptions"/>
        public string ObscureStringForInvalidOption { get; set; } = "****";

        /// <summary>
        /// Obscures the options in the error description to hide the sensitive data. The default value is <c>true</c>.
        /// </summary>
        /// <remarks>Use <see cref="ObscureStringForInvalidOption"/> to configure the obscure string.</remarks>
        /// <seealso cref="ObscureStringForInvalidOption"/>
        public bool ObsureInvalidOptions { get; set; } = true;

        /// <summary>
        /// The indent size for the terminal logger messages.
        /// </summary>
        public int LoggerIndent { get; set; } = 4;

        /// <summary>
        /// Logs all the terminal messages to standard logger.
        /// </summary>
        public bool? LogToStandard { get; set; }
    }
}