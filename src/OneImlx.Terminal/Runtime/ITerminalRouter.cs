/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;
using OneImlx.Terminal.Configuration.Options;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// An abstraction of a context aware terminal router.
    /// </summary>
    /// <remarks>
    /// This terminal router runs indefinitely until it receives a cancellation request or when the application stops.
    /// It is responsible for receiving commands and routing them to the appropriate command routers based on the
    /// context. The operation is asynchronous, continuously handling incoming commands and delegating them as long as
    /// the router is running.
    /// </remarks>
    public interface ITerminalRouter<TContext> where TContext : TerminalRouterContext
    {
        /// <summary>
        /// Gets a value indicating whether the terminal router is running.
        /// </summary>
        public bool IsRunning { get; }

        /// <summary>
        /// The terminal router name.
        /// </summary>
        /// <remarks>The <see cref="Name"/> property is used for licensing checks and should match <see cref="RouterOptions.Name"/>.</remarks>
        public string Name { get; }

        /// <summary>
        /// Runs terminal router asynchronously.
        /// </summary>
        /// <param name="context">The terminal router context.</param>
        public Task RunAsync(TContext context);
    }
}
