/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The default <see cref="ITerminalRouter{TContext}"/> for custom routing.
    /// </summary>
    public abstract class TerminalCustomRouter : ITerminalRouter<TerminalCustomRouterContext>
    {
        /// <summary>
        /// Gets a value indicating whether the console terminal is running.
        /// </summary>
        public abstract bool IsRunning { get; }

        /// <summary>
        /// The terminal router name.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Routes to a custom service implementation.
        /// </summary>
        /// <param name="context">The custom routing service context.</param>
        public abstract Task RunAsync(TerminalCustomRouterContext context);
    }
}
