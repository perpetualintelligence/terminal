/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// The option extractor context.
    /// </summary>
    public class ArgumentExtractorContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="argumentString">The option string.</param>
        /// <param name="commandDescriptor">The command descriptor.</param>

        public ArgumentExtractorContext(OptionString argumentString, CommandDescriptor commandDescriptor)
        {
            ArgumentString = argumentString ?? throw new ArgumentNullException(nameof(argumentString));
            CommandDescriptor = commandDescriptor ?? throw new ArgumentNullException(nameof(commandDescriptor));
        }

        /// <summary>
        /// The option string.
        /// </summary>
        public OptionString ArgumentString { get; set; }

        /// <summary>
        /// The command descriptor.
        /// </summary>
        public CommandDescriptor CommandDescriptor { get; set; }
    }
}
