/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// The argument extractor context.
    /// </summary>
    public class ArgumentExtractorContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="argumentString">The argument string.</param>
        /// <param name="commandDescriptor">The command descriptor.</param>

        public ArgumentExtractorContext(ArgumentString argumentString, CommandDescriptor commandDescriptor)
        {
            ArgumentString = argumentString ?? throw new ArgumentNullException(nameof(argumentString));
            CommandDescriptor = commandDescriptor ?? throw new ArgumentNullException(nameof(commandDescriptor));
        }

        /// <summary>
        /// The argument string.
        /// </summary>
        public ArgumentString ArgumentString { get; set; }

        /// <summary>
        /// The command descriptor.
        /// </summary>
        public CommandDescriptor CommandDescriptor { get; set; }
    }
}
