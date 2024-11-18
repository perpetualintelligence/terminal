/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

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
        public TerminalExceptionHandlerContext(Exception exception, TerminalRequest? request = null)
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
        public TerminalRequest? Request { get; }
    }
}
