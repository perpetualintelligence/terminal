/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Infrastructure;

namespace PerpetualIntelligence.Cli.Commands
{
    /// <summary>
    /// The default command extractor result.
    /// </summary>
    public class CommandExtractorResult : OneImlxResult
    {
        /// <summary>
        /// The extracted command.
        /// </summary>
        public Command? Command { get; set; }
    }
}
