/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Shared.Infrastructure;

namespace OneImlx.Terminal
{
    /// <summary>
    /// The exception that represents a terminal error.
    /// </summary>
    public sealed class TerminalException : ErrorException
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
