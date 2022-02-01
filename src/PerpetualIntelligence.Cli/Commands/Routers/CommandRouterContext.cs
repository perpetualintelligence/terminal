﻿/*
    Copyright 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System.Threading;

namespace PerpetualIntelligence.Cli.Commands.Routers
{
    /// <summary>
    /// The <c>cli</c> generic command router context.
    /// </summary>
    public class CommandRouterContext
    {
        /// <summary>
        /// The command string.
        /// </summary>
        /// <param name="rawCommandString">The raw command string.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public CommandRouterContext(string rawCommandString, CancellationToken? cancellationToken = null)
        {
            if (string.IsNullOrWhiteSpace(rawCommandString))
            {
                throw new System.ArgumentException($"'{nameof(rawCommandString)}' cannot be null or whitespace.", nameof(rawCommandString));
            }

            RawCommandString = rawCommandString;
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// The cancellation token.
        /// </summary>
        public CancellationToken? CancellationToken { get; protected set; }

        /// <summary>
        /// The raw command string.
        /// </summary>
        public string RawCommandString { get; protected set; }
    }
}
