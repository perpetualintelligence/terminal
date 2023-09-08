/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Extractors;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Mocks
{
    public class MockArgumentExtractor : IOptionExtractor
    {
        public bool Called { get; set; }

        public Task<OptionExtractorResult> ExtractAsync(OptionExtractorContext context)
        {
            Called = true;
            return Task.FromResult(new OptionExtractorResult(new Commands.Option(new Commands.OptionDescriptor("testid", nameof(String), "testdesc", Commands.OptionFlags.None), "testvalue")));
        }
    }
}