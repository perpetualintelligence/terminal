/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The arguments extractor context.
    /// </summary>
    public class ArgumentsExtractorContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="commandIdentity">The command identity.</param>
        /// <param name="arguments">The arguments string.</param>
        public ArgumentsExtractorContext(CommandIdentity commandIdentity, string arguments)
        {
            if (string.IsNullOrWhiteSpace(arguments))
            {
                throw new ArgumentException($"'{nameof(arguments)}' cannot be null or whitespace.", nameof(arguments));
            }

            Arguments = arguments;
            CommandIdentity = commandIdentity ?? throw new ArgumentNullException(nameof(commandIdentity));
        }

        /// <summary>
        /// The arguments to extract.
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// The command identity.
        /// </summary>
        public CommandIdentity CommandIdentity { get; set; }
    }
}
