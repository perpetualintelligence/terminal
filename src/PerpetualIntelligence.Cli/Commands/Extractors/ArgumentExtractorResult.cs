/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Infrastructure;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// The option extractor result.
    /// </summary>
    public class ArgumentExtractorResult
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="option"></param>
        public ArgumentExtractorResult(Option option)
        {
            Argument = option ?? throw new System.ArgumentNullException(nameof(option));
        }

        /// <summary>
        /// The extracted option.
        /// </summary>
        public Option Argument { get; set; }
    }
}
