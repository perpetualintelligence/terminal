/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
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
