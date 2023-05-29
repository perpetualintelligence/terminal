/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    /// <summary>
    /// The command extractor context.
    /// </summary>
    public class CommandExtractorContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="commandRoute">The command route.</param>
        public CommandExtractorContext(CommandRoute commandRoute)
        {
            Route = commandRoute ?? throw new ArgumentNullException(nameof(commandRoute));
        }

        /// <summary>
        /// The command route.
        /// </summary>
        public CommandRoute Route { get; set; }
    }
}