/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Infrastructure;
using System;

namespace PerpetualIntelligence.Cli.Commands.Publishers
{
    /// <summary>
    /// The <see cref="IExceptionPublisher"/> context.
    /// </summary>
    public class ErrorPublisherContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="rawCommandString">The raw command string.</param>
        /// <param name="error">The error.</param>
        public ErrorPublisherContext(string rawCommandString, Error error)
        {
            RawCommandString = rawCommandString ?? throw new ArgumentNullException(nameof(rawCommandString));
            Error = error ?? throw new ArgumentNullException(nameof(error));
        }

        /// <summary>
        /// The error.
        /// </summary>
        public Error Error { get; set; }

        /// <summary>
        /// The command string.
        /// </summary>
        public string RawCommandString { get; set; }
    }
}
