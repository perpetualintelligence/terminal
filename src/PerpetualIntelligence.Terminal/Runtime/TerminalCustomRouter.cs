/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalRouter{TContext}"/> for custom routing.
    /// </summary>
    public abstract class TerminalCustomRouter : ITerminalRouter<TerminalCustomRouterContext>
    {
        /// <summary>
        /// Routes to a custom service implementation.
        /// </summary>
        /// <param name="context">The custom routing service context.</param>
        public abstract Task RunAsync(TerminalCustomRouterContext context);
    }
}