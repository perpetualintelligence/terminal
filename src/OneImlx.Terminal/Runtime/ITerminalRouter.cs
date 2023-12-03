/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// An abstraction of a context aware terminal router.
    /// </summary>
    /// <remarks>
    /// This terminal router runs indefinitely until it receives a cancellation request
    /// or when the application stops. It is responsible for receiving commands and routing
    /// them to the appropriate command routers based on the context. The operation is
    /// asynchronous, continuously handling incoming commands and delegating them as long
    /// as the router is running.
    /// </remarks>
    public interface ITerminalRouter<TContext> where TContext : TerminalRouterContext
    {
        /// <summary>
        /// Runs terminal router asynchronously.
        /// </summary>
        /// <param name="context">The terminal router context.</param>
        public Task RunAsync(TContext context);
    }
}