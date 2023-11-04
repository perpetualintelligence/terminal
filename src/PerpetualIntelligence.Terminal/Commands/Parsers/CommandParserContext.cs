/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace PerpetualIntelligence.Terminal.Commands.Parsers
{
    /// <summary>
    /// The command parser context.
    /// </summary>
    public sealed class CommandParserContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="commandRoute">The command route.</param>
        public CommandParserContext(CommandRoute commandRoute)
        {
            Route = commandRoute ?? throw new ArgumentNullException(nameof(commandRoute));
        }

        /// <summary>
        /// The command route.
        /// </summary>
        public CommandRoute Route { get; set; }
    }
}