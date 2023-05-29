/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

namespace PerpetualIntelligence.Terminal.Commands.Extractors
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
        public CommandExtractorResult(Command command)
        {
            Command = command ?? throw new System.ArgumentNullException(nameof(command));
        }

        /// <summary>
        /// The extracted command.
        /// </summary>
        public Command Command { get; }
    }
}