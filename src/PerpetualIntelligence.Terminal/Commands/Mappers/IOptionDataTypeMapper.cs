/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Abstractions;

namespace PerpetualIntelligence.Terminal.Commands.Mappers
{
    /// <summary>
    /// An abstraction to map an <see cref="Option.DataType"/> to <see cref="System.Type"/>.
    /// </summary>
    public interface IOptionDataTypeMapper : IMapper<OptionDataTypeMapperContext, OptionDataTypeMapperResult>
    {
    }
}
