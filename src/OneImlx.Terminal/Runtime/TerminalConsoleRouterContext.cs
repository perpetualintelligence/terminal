/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace OneImlx.Terminal.Runtime
{
    /// <summary>
    /// The <see cref="TerminalRouterContext"/> for <see cref="TerminalConsoleRouter"/>.
    /// </summary>
    public sealed class TerminalConsoleRouterContext : TerminalRouterContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="startContext">The terminal start context.</param>
        public TerminalConsoleRouterContext(TerminalStartContext startContext) : base(startContext)
        {
        }
    }
}