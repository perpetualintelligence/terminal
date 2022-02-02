/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Infrastructure;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// The argument extractor result.
    /// </summary>
    public class ArgumentExtractorResult
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="argument"></param>
        public ArgumentExtractorResult(Argument argument)
        {
            Argument = argument ?? throw new System.ArgumentNullException(nameof(argument));
        }

        /// <summary>
        /// The extracted argument.
        /// </summary>
        public Argument Argument { get; set; }
    }
}
