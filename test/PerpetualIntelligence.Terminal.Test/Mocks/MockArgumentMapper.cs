/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Cli.Commands.Mappers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockArgumentMapper : IOptionDataTypeMapper
    {
        public bool Called { get; set; }

        public Task<OptionDataTypeMapperResult> MapAsync(OptionDataTypeMapperContext context)
        {
            Called = true;
            return Task.FromResult(new OptionDataTypeMapperResult(typeof(string)));
        }
    }
}
