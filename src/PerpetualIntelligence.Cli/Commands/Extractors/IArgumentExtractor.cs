/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Abstractions;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// An abstraction to extract an argument.
    /// </summary>
    public interface IArgumentExtractor : IExtractor<ArgumentExtractorContext, ArgumentExtractorResult>
    {
    }
}
