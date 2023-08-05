/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace PerpetualIntelligence.Terminal.Commands.Handlers
{
    /// <summary>
    /// The <see cref="IExceptionHandler"/> context.
    /// </summary>
    public class ExceptionHandlerContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="rawCommandString">The raw command string.</param>
        public ExceptionHandlerContext(Exception exception, string? rawCommandString)
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
        public string? RawCommandString { get; set; }
    }
}