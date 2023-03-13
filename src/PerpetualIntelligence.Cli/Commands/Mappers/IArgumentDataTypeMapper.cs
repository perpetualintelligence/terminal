/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Abstractions;

namespace PerpetualIntelligence.Cli.Commands.Mappers
{
    /// <summary>
    /// An abstraction to map an <see cref="Option.DataType"/> to <see cref="System.Type"/>.
    /// </summary>
    public interface IArgumentDataTypeMapper : IMapper<ArgumentDataTypeMapperContext, ArgumentDataTypeMapperResult>
    {
    }
}
