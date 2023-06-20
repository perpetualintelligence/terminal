/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Mappers
{
    /// <summary>
    /// An abstraction to map an <see cref="Option.DataType"/> to <see cref="System.Type"/>.
    /// </summary>
    public interface IOptionDataTypeMapper
    {
        /// <summary>
        /// Maps an <see cref="Option.DataType"/> to <see cref="System.Type"/> asynchronously.
        /// </summary>
        /// <param name="context">The map context.</param>
        /// <returns>The </returns>
        public Task<OptionDataTypeMapperResult> MapAsync(OptionDataTypeMapperContext context);
    }
}