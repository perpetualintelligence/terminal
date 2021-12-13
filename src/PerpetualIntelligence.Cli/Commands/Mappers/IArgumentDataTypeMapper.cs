/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
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
