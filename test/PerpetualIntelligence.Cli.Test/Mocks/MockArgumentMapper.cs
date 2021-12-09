/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Mappers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockArgumentMapper : IArgumentMapper
    {
        public bool Called { get; set; }

        public Task<DataAnnotationsMapperTypeResult> MapAsync(DataAnnotationsMapperTypeContext context)
        {
            Called = true;
            return Task.FromResult(new DataAnnotationsMapperTypeResult());
        }
    }
}
