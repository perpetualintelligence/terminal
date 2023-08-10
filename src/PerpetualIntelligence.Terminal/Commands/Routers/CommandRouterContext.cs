/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Threading;

namespace PerpetualIntelligence.Terminal.Commands.Routers
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
                throw new ArgumentException($"'{nameof(rawCommandString)}' cannot be null or whitespace.", nameof(rawCommandString));
            }

            CancellationToken = cancellationToken;
            Route = new CommandRoute(Guid.NewGuid().ToString(), rawCommandString);
        }

        /// <summary>
        /// The cancellation token.
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// The command route.
        /// </summary>
        public CommandRoute Route { get; }
    }
}