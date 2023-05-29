/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Cli.Commands.Extractors;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockArgumentExtractor : IOptionExtractor
    {
        public bool Called { get; set; }

        public Task<OptionExtractorResult> ExtractAsync(OptionExtractorContext context)
        {
            Called = true;
            return Task.FromResult(new OptionExtractorResult(new Commands.Option(new Commands.OptionDescriptor("testid", System.ComponentModel.DataAnnotations.DataType.Text, "testdesc"), "testvalue")));
        }
    }
}