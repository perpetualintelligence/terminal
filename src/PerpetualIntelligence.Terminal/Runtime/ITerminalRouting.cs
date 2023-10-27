/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// An abstraction of a context aware terminal routing.
    /// </summary>
    public interface ITerminalRouting<TContext> where TContext : TerminalRoutingContext
    {
        /// <summary>
        /// Runs terminal routing asynchronously.
        /// </summary>
        /// <param name="context">The terminal router context.</param>
        public Task RunAsync(TContext context);
    }
}