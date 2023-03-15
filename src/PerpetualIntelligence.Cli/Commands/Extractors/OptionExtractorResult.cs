/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Infrastructure;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// The option extractor result.
    /// </summary>
    public class OptionExtractorResult
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="option"></param>
        public OptionExtractorResult(Option option)
        {
            Option = option ?? throw new System.ArgumentNullException(nameof(option));
        }

        /// <summary>
        /// The extracted option.
        /// </summary>
        public Option Option { get; set; }
    }
}
