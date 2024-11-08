/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The <see cref="ITerminalExceptionHandler"/> context.
    /// </summary>
    public class TerminalExceptionHandlerContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="request">The command request.</param>
        public TerminalExceptionHandlerContext(Exception exception, TerminalProcessorRequest? request = null)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
            Request = request;
        }

        /// <summary>
        /// The exception.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// The command request.
        /// </summary>
        public TerminalProcessorRequest? Request { get; }
    }
}