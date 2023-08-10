/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// The <see cref="TerminalRoutingContext"/> for <see cref="TerminalConsoleRouting"/>.
    /// </summary>
    public sealed class TerminalConsoleRoutingContext : TerminalRoutingContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="startContext">The terminal start context.</param>
        public TerminalConsoleRoutingContext(TerminalStartContext startContext) : base(startContext)
        {
        }
    }
}