/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Abstractions;

namespace PerpetualIntelligence.Cli.Commands.Mappers
{
    /// <summary>
    /// An abstraction to map an <see cref="Argument.DataType"/> to <see cref="System.Type"/>.
    /// </summary>
    public interface IArgumentDataTypeMapper : IMapper<ArgumentDataTypeMapperContext, ArgumentDataTypeMapperResult>
    {
    }
}
