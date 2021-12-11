/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Infrastructure;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// The argument extractor result.
    /// </summary>
    public class ArgumentExtractorResult : OneImlxResult
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
