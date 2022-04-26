/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Infrastructure;
using System;

namespace PerpetualIntelligence.Cli.Commands.Handlers
{
    /// <summary>
    /// The <see cref="IExceptionHandler"/> context.
    /// </summary>
    public class ErrorHandlerContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="error">The error.</param>
        public ErrorHandlerContext(Error error)
        {
            Error = error;
        }

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="rawCommandString">The raw command string.</param>
        public ErrorHandlerContext(Error error, string rawCommandString)
        {
            RawCommandString = rawCommandString ?? throw new ArgumentNullException(nameof(rawCommandString));
            Error = error ?? throw new ArgumentNullException(nameof(error));
        }

        /// <summary>
        /// The error.
        /// </summary>
        public Error Error { get; set; }

        /// <summary>
        /// The raw command string.
        /// </summary>
        public string? RawCommandString { get; set; }
    }
}
