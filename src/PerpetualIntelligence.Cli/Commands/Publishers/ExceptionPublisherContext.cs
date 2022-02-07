/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Commands.Publishers
{
    /// <summary>
    /// The <see cref="IExceptionPublisher"/> context.
    /// </summary>
    public class ExceptionPublisherContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="rawCommandString">The raw command string.</param>
        /// <param name="exception">The exception.</param>
        public ExceptionPublisherContext(string rawCommandString, Exception exception)
        {
            RawCommandString = rawCommandString ?? throw new ArgumentNullException(nameof(rawCommandString));
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        }

        /// <summary>
        /// The exception.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// The command string.
        /// </summary>
        public string RawCommandString { get; set; }
    }
}
