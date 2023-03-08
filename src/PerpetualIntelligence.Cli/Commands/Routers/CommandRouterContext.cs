/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;
using System.Threading;

namespace PerpetualIntelligence.Cli.Commands.Routers
{
    /// <summary>
    /// The <c>pi-cli</c> generic command router context.
    /// </summary>
    public sealed class CommandRouterContext
    {
        /// <summary>
        /// The command string.
        /// </summary>
        /// <param name="rawCommandString">The raw command string.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public CommandRouterContext(string rawCommandString, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(rawCommandString))
            {
                throw new System.ArgumentException($"'{nameof(rawCommandString)}' cannot be null or whitespace.", nameof(rawCommandString));
            }

            RawCommandString = rawCommandString;
            CancellationToken = cancellationToken;
            Route = new CommandRoute(Guid.NewGuid().ToString());
        }

        /// <summary>
        /// The cancellation token.
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// The raw command string.
        /// </summary>
        public string RawCommandString { get; }

        /// <summary>
        /// The command route.
        /// </summary>
        public CommandRoute Route { get; }
    }
}