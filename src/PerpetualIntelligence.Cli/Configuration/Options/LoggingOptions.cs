/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

namespace PerpetualIntelligence.Cli.Configuration.Options
{
    /// <summary>
    /// The logging configuration options.
    /// </summary>
    public class LoggingOptions : Shared.Infrastructure.LoggingOptions
    {
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