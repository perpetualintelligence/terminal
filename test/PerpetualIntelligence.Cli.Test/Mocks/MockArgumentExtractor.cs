/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Extractors;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockArgumentExtractor : IArgumentExtractor
    {
        public bool Called { get; set; }

        public Task<ArgumentExtractorResult> ExtractAsync(ArgumentExtractorContext context)
        {
            Called = true;
            return Task.FromResult(new ArgumentExtractorResult(new Commands.Argument(new Commands.ArgumentDescriptor("testid", System.ComponentModel.DataAnnotations.DataType.Text, "testdesc"), "testvalue")));
        }
    }
}