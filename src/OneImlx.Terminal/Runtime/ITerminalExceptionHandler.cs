/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// An abstraction to handle <see cref="Exception"/>.
    /// </summary>
    public interface ITerminalExceptionHandler
    {
        /// <summary>
        /// Handles <see cref="Exception"/> asynchronously.
        /// </summary>
        /// <param name="context">The exception context.</param>
        public Task HandleExceptionAsync(TerminalExceptionHandlerContext context);
    }
}