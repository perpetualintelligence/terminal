/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
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
        public ArgumentExtractorContext(string argumentString, CommandDescriptor commandDescriptor) : this(argumentString, isAlias: false, commandDescriptor)
        {
        }

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="argumentString">The argument string.</param>
        /// <param name="isAlias"><c>true</c> if the argument is identified by its alias, otherwise <c>false</c>.</param>
        /// <param name="commandDescriptor">The command descriptor.</param>
        public ArgumentExtractorContext(string argumentString, bool isAlias, CommandDescriptor commandDescriptor)
        {
            if (string.IsNullOrWhiteSpace(argumentString))
            {
                throw new System.ArgumentException($"'{nameof(argumentString)}' cannot be null or whitespace.", nameof(argumentString));
            }

            ArgumentString = argumentString;
            IsAlias = isAlias;
            CommandDescriptor = commandDescriptor ?? throw new System.ArgumentNullException(nameof(commandDescriptor));
        }

        /// <summary>
        /// The argument string.
        /// </summary>
        public string ArgumentString { get; set; }

        /// <summary>
        /// The command descriptor.
        /// </summary>
        public CommandDescriptor CommandDescriptor { get; set; }

        /// <summary>
        /// <c>true</c> if the argument is identified by its alias, otherwise <c>false</c>.
        /// </summary>
        public bool IsAlias { get; }
    }
}
