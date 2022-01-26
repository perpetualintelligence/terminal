/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Infrastructure;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// The command extractor result.
    /// </summary>
    public class CommandExtractorResult : ResultNoError
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="command">The extracted command.</param>
        /// <param name="commandIdentity">The extracted command identity.</param>
        public CommandExtractorResult(Command command, CommandIdentity commandIdentity)
        {
            Command = command ?? throw new System.ArgumentNullException(nameof(command));
            CommandIdentity = commandIdentity ?? throw new System.ArgumentNullException(nameof(commandIdentity));
        }

        /// <summary>
        /// The extracted command.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        /// The extracted command identity.
        /// </summary>
        public CommandIdentity CommandIdentity { get; }
    }
}
