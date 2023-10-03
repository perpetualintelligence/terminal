/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Infrastructure;
using System;

namespace PerpetualIntelligence.Shared.Exceptions
{
    /// <summary>
    /// The exception that represents a terminal error.
    /// </summary>
    public class TerminalException : Exception
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        public TerminalException()
        {
            Error = new Error(Error.Unexpected, "");
        }

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="error">The error.</param>
        public TerminalException(Error error)
        {
            Error = error ?? throw new ArgumentNullException(nameof(error));
        }

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public TerminalException(string message) : base(message)
        {
            Error = new Error(Error.Unexpected, "");
        }

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="error">The error code.</param>
        /// <param name="errorDescription">The error description.</param>
        public TerminalException(string error, string errorDescription)
        {
            Error = new Error(error, errorDescription);
        }

        /// <summary>
        /// Initializes a new error exception.
        /// </summary>
        /// <param name="error">The error code.</param>
        /// <param name="errorDescription">The error description.</param>
        /// <param name="args">The error description format arguments.</param>
        public TerminalException(string error, string errorDescription, params object?[] args)
        {
            Error = new Error(error, errorDescription, args);
        }

        /// <summary>
        /// The error.
        /// </summary>
        public Error Error { get; set; }

        /// <summary>
        /// The exception message.
        /// </summary>
        public override string Message
        {
            get
            {
                // FOMAC: we need to make sure ErrorDescription is never null
                if (Error.ErrorDescription == null)
                {
                    return base.Message;
                }
                else
                {
                    return Error.FormatDescription();
                }
            }
        }
    }
}