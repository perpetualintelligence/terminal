/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    /// <summary>
    /// The option extractor context.
    /// </summary>
    public class OptionExtractorContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="argumentString">The option string.</param>
        /// <param name="commandDescriptor">The command descriptor.</param>

        public OptionExtractorContext(OptionString argumentString, CommandDescriptor commandDescriptor)
        {
            OptionString = argumentString ?? throw new ArgumentNullException(nameof(argumentString));
            CommandDescriptor = commandDescriptor ?? throw new ArgumentNullException(nameof(commandDescriptor));
        }

        /// <summary>
        /// The option string.
        /// </summary>
        public OptionString OptionString { get; set; }

        /// <summary>
        /// The command descriptor.
        /// </summary>
        public CommandDescriptor CommandDescriptor { get; set; }
    }
}
