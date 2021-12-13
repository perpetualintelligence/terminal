/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Abstractions;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// An abstraction to extract a command.
    /// </summary>
    public interface ICommandExtractor : IExtractor<CommandExtractorContext, CommandExtractorResult>
    {
    }
}
