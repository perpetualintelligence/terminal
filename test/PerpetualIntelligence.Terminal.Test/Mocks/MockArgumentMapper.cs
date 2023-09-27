﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands;
using PerpetualIntelligence.Terminal.Commands.Mappers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Mocks
{
    public class MockArgumentMapper : IDataTypeMapper<Argument>
    {
        public bool Called { get; set; }

        public Task<DataTypeMapperResult> MapAsync(DataTypeMapperContext<Argument> context)
        {
            Called = true;
            return Task.FromResult(new DataTypeMapperResult(typeof(string)));
        }
    }
}