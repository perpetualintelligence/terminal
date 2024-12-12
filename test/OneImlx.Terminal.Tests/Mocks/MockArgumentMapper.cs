/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Mappers;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Mocks
{
    public class MockArgumentMapper : IDataTypeMapper<Argument>
    {
        public bool Called { get; set; }

        public Task<DataTypeMapperResult> MapToTypeAsync(Argument argument)
        {
            Called = true;
            return Task.FromResult(new DataTypeMapperResult(typeof(string)));
        }
    }
}
