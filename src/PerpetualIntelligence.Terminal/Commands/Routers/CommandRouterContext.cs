/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Runtime;
using System;

namespace PerpetualIntelligence.Terminal.Commands.Routers
{
    /// <summary>
    /// The generic command router context.
    /// </summary>
    public sealed class CommandRouterContext
    {
        /// <summary>
        /// The command string.
        /// </summary>
        /// <param name="rawCommandString">The raw command string.</param>
        /// <param name="routingContext">The terminal routing context.</param>
        public CommandRouterContext(string rawCommandString, TerminalRoutingContext routingContext)
        {
            if (string.IsNullOrWhiteSpace(rawCommandString))
            {
                throw new ArgumentException($"'{nameof(rawCommandString)}' cannot be null or whitespace.", nameof(rawCommandString));
            }

            RoutingContext = routingContext;
            Route = new CommandRoute(Guid.NewGuid().ToString(), rawCommandString);
        }

        /// <summary>
        /// The terminal routing context.
        /// </summary>
        public TerminalRoutingContext RoutingContext { get; }

        /// <summary>
        /// The command route.
        /// </summary>
        public CommandRoute Route { get; }
    }
}