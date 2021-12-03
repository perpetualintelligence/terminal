/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Abstractions;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// An abstraction to check argument syntax.
    /// </summary>
    public interface IArgumentExtractor : IExtractor<ArgumentExtractorContext, ArgumentExtractorResult>
    {
    }
}
