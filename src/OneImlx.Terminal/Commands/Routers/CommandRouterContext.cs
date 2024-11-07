/*
    Copyright 2024 (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Commands.Routers
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
        /// <param name="properties">The additional router properties.</param>
        public CommandRouterContext(string rawCommandString, TerminalRouterContext routingContext, Dictionary<string, object>? properties)
        {
            if (string.IsNullOrWhiteSpace(rawCommandString))
            {
                throw new ArgumentException($"'{nameof(rawCommandString)}' cannot be null or whitespace.", nameof(rawCommandString));
            }

            TerminalContext = routingContext;
            Properties = properties;
            Request = new TerminalProcessorRequest(Guid.NewGuid().ToString(), rawCommandString);
        }

        /// <summary>
        /// The additional router properties.
        /// </summary>
        public Dictionary<string, object>? Properties { get; }

        /// <summary>
        /// The command route.
        /// </summary>
        public TerminalProcessorRequest Request { get; }

        /// <summary>
        /// The terminal routing context.
        /// </summary>
        public TerminalRouterContext TerminalContext { get; }
    }
}
