/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using OneImlx.Shared.Extensions;
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
        /// <param name="request">The request to process.</param>
        /// <param name="context">The terminal routing context.</param>
        /// <param name="properties">The additional router properties.</param>
        public CommandRouterContext(TerminalRequest request, TerminalRouterContext context, Dictionary<string, object>? properties)
        {
            TerminalContext = context ?? throw new ArgumentNullException(nameof(context));
            Properties = properties;
            Request = request ?? throw new ArgumentNullException(nameof(request));
        }

        /// <summary>
        /// The additional router properties.
        /// </summary>
        public Dictionary<string, object>? Properties { get; }

        /// <summary>
        /// The command request.
        /// </summary>
        public TerminalRequest Request { get; }

        /// <summary>
        /// The terminal routing context.
        /// </summary>
        public TerminalRouterContext TerminalContext { get; }
    }
}
