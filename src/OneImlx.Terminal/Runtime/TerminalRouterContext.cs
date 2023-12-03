/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The <see cref="ITerminalRouter{TContext}"/> context.
    /// </summary>
    public abstract class TerminalRouterContext
    {
        /// <summary>
        /// The terminal start context.
        /// </summary>
        public TerminalStartContext StartContext { get; }

        /// <summary>
        /// Initializes a new <see cref="TerminalRouterContext"/> instance.
        /// </summary>
        /// <param name="startContext">The terminal start context.</param>
        protected TerminalRouterContext(TerminalStartContext startContext)
        {
            StartContext = startContext;
        }
    }
}