/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Infrastructure;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// The argument extractor result.
    /// </summary>
    public class ArgumentExtractorResult : Result
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        public ArgumentExtractorResult()
        {
        }

        /// <summary>
        /// The extracted argument.
        /// </summary>
        public Argument? Argument { get; set; }
    }
}
