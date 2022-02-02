/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Infrastructure;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// The command extractor result.
    /// </summary>
    public class CommandExtractorResult
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="command">The extracted command.</param>
        /// <param name="commandDescriptor">The extracted command descriptor.</param>
        public CommandExtractorResult(Command command, CommandDescriptor commandDescriptor)
        {
            Command = command ?? throw new System.ArgumentNullException(nameof(command));
            CommandDescriptor = commandDescriptor ?? throw new System.ArgumentNullException(nameof(commandDescriptor));
        }

        /// <summary>
        /// The extracted command.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        /// The extracted command descriptor.
        /// </summary>
        public CommandDescriptor CommandDescriptor { get; }
    }
}
