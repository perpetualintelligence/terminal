/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

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
        public ArgumentExtractorContext(string argumentString, CommandDescriptor commandDescriptor)
        {
            ArgumentString = argumentString;
            CommandDescriptor = commandDescriptor;
        }

        /// <summary>
        /// The argument string.
        /// </summary>
        public string ArgumentString { get; set; }

        /// <summary>
        /// The command descriptor.
        /// </summary>
        public CommandDescriptor CommandDescriptor { get; set; }
    }
}
