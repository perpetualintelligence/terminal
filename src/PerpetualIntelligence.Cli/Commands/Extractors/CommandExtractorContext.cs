/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
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
        public CommandExtractorContext(string commandString)
        {
            if (string.IsNullOrWhiteSpace(commandString))
            {
                throw new ArgumentException($"'{nameof(commandString)}' cannot be null or whitespace.", nameof(commandString));
            }

            CommandString = commandString;
        }

        /// <summary>
        /// The command string.
        /// </summary>
        public string CommandString { get; set; }
    }
}
