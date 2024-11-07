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
        /// <param name="commandRoute">The command route.</param>
        public TerminalExceptionHandlerContext(Exception exception, TerminalProcessorRequest? commandRoute = null)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
            Route = commandRoute;
        }

        /// <summary>
        /// The exception.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// The command route.
        /// </summary>
        public TerminalProcessorRequest? Route { get; }
    }
}