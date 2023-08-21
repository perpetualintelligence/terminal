/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

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
        /// <param name="extractedCommand">The extracted command.</param>
        public CommandExtractorResult(ExtractedCommand extractedCommand)
        {
            ExtractedCommand = extractedCommand ?? throw new System.ArgumentNullException(nameof(extractedCommand));
        }

        /// <summary>
        /// The extracted command.
        /// </summary>
        public ExtractedCommand ExtractedCommand { get; }
    }
}