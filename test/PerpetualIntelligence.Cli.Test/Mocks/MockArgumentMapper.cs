/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Mappers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockArgumentMapper : IArgumentDataTypeMapper
    {
        public bool Called { get; set; }

        public Task<ArgumentDataTypeMapperResult> MapAsync(ArgumentDataTypeMapperContext context)
        {
            Called = true;
            return Task.FromResult(new ArgumentDataTypeMapperResult(typeof(string)));
        }
    }
}
