/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Infrastructure;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The default arguments extractor result.
    /// </summary>
    public class ArgumentsExtractorResult : OneImlxResult
    {
        /// <summary>
        /// The extracted arguments.
        /// </summary>
        public Arguments? Arguments { get; set; }
    }
}
