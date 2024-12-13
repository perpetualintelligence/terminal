/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Checkers
{
    /// <summary>
    /// An abstraction to map an <see cref="Option.DataType"/> to <see cref="System.Type"/>.
    /// </summary>
    public interface IDataTypeMapper<TValue> where TValue : ICommandValue
    {
        /// <summary>
        /// Maps a data type to <see cref="System.Type"/> asynchronously.
        /// </summary>
        /// <param name="value">The value to map.</param>
        /// <returns>The</returns>
        public Task<DataTypeMapperResult> MapToTypeAsync(TValue value);
    }
}
