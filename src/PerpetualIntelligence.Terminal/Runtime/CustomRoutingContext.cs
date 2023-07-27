/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// The <see cref="TerminalRoutingContext"/> for <see cref="CustomRoutingContext"/>.
    /// </summary>
    public abstract class CustomRoutingContext : TerminalRoutingContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="startContext">The terminal start context.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        public CustomRoutingContext(TerminalStartContext startContext, CancellationToken cancellationToken) : base(startContext, cancellationToken)
        {
        }
    }
}