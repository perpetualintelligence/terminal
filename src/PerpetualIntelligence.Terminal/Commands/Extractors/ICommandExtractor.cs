/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Abstractions;

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    /// <summary>
    /// An abstraction to extract a command.
    /// </summary>
    public interface ICommandExtractor : IExtractor<CommandExtractorContext, CommandExtractorResult>
    {
    }
}
