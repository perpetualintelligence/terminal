﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Runtime
{
    /// <summary>
    /// The <see cref="TerminalRoutingContext"/> for <see cref="ConsoleRouting"/>.
    /// </summary>
    public sealed class ConsoleRoutingContext : TerminalRoutingContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="startContext">The terminal start context.</param>
        public ConsoleRoutingContext(TerminalStartContext startContext) : base(startContext)
        {
        }
    }
}