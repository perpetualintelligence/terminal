/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// The <see cref="ITerminalRouting{TContext, TResult}"/> context.
    /// </summary>
    public abstract class TerminalRoutingContext
    {
        /// <summary>
        /// The cancellation token.
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// The terminal start context.
        /// </summary>
        public TerminalStartContext StartContext { get; }

        /// <summary>
        /// Initializes a new <see cref="TerminalRoutingContext"/> instance.
        /// </summary>
        /// <param name="startContext">The terminal start context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected TerminalRoutingContext(TerminalStartContext startContext, CancellationToken cancellationToken)
        {
            StartContext = startContext;
            CancellationToken = cancellationToken;
        }
    }
}