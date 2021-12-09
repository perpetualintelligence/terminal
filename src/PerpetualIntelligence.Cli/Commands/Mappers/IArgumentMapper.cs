/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Protocols.Abstractions;

namespace PerpetualIntelligence.Cli.Commands.Mappers
{
    /// <summary>
    /// An abstraction to extract a data type of an argument.
    /// </summary>
    public interface IArgumentMapper : IMapper<DataAnnotationsMapperTypeContext, DataAnnotationsMapperTypeResult>
    {
    }
}
