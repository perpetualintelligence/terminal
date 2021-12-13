/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Abstractions;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// An abstraction to extract an argument.
    /// </summary>
    public interface IArgumentExtractor : IExtractor<ArgumentExtractorContext, ArgumentExtractorResult>
    {
    }
}
