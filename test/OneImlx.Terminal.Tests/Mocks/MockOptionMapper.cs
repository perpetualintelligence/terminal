﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Mappers;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Mocks
{
    public class MockOptionMapper : IDataTypeMapper<Option>
    {
        public bool Called { get; set; }

        public Task<DataTypeMapperResult> MapToTypeAsync(DataTypeMapperContext<Option> context)
        {
            Called = true;
            return Task.FromResult(new DataTypeMapperResult(typeof(string)));
        }
    }
}