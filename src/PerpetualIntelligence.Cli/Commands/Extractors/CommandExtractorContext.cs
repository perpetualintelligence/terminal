/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// The command extractor context.
    /// </summary>
    public class CommandExtractorContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="commandString">The command string.</param>
        public CommandExtractorContext(CommandString commandString)
        {
            CommandString = commandString ?? throw new ArgumentNullException(nameof(commandString));
        }

        /// <summary>
        /// The command string.
        /// </summary>
        public CommandString CommandString { get; set; }
    }
}
