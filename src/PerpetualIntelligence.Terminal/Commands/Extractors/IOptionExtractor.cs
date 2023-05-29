/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Abstractions;

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    /// <summary>
    /// An abstraction to extract an option.
    /// </summary>
    public interface IOptionExtractor : IExtractor<OptionExtractorContext, OptionExtractorResult>
    {
    }
}
