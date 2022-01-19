/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Infrastructure;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// The command extractor result.
    /// </summary>
    public class CommandExtractorResult : Result
    {
        /// <summary>
        /// The extracted command.
        /// </summary>
        public Command? Command { get; set; }

        /// <summary>
        /// The extracted command identity.
        /// </summary>
        public CommandIdentity? CommandIdentity { get; set; }
    }
}
