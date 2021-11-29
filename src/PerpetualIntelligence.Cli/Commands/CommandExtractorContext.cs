/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The default command extractor context.
    /// </summary>
    public class CommandExtractorContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="command"></param>
        public CommandExtractorContext(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentException($"'{nameof(command)}' cannot be null or whitespace.", nameof(command));
            }

            Command = command;
        }

        /// <summary>
        /// The command to extract.
        /// </summary>
        public string Command { get; set; }
    }
}
