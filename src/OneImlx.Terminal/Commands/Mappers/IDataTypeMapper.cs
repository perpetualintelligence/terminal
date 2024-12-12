/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Mappers
{
    /// <summary>
    /// An abstraction to map an <see cref="Option.DataType"/> to <see cref="System.Type"/>.
    /// </summary>
    public interface IDataTypeMapper<TValue> where TValue : ICommandValue
    {
        /// <summary>
        /// Maps a data type to <see cref="System.Type"/> asynchronously.
        /// </summary>
        /// <param name="context">The map context.</param>
        /// <returns>The </returns>
        public Task<DataTypeMapperResult> MapToTypeAsync(DataTypeMapperContext<TValue> context);
    }
}