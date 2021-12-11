/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
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
            return Task.FromResult(new ArgumentDataTypeMapperResult());
        }
    }
}
