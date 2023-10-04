/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Infrastructure;

namespace PerpetualIntelligence.Terminal
{
    /// <summary>
    /// The exception that represents a terminal error.
    /// </summary>
    public class TerminalException : ErrorException
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="error">The error code.</param>
        /// <param name="errorDescription">The error description.</param>
        public TerminalException(string error, string errorDescription) : base(error, errorDescription)
        {
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="error">The error.</param>
        public TerminalException(Error error) : base(error)
        {
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="error">The error code.</param>
        /// <param name="errorDescription">The error description.</param>
        /// <param name="args">The error description format arguments.</param>
        public TerminalException(string error, string errorDescription, params object?[] args) : base(error, errorDescription, args)
        {
        }
    }
}