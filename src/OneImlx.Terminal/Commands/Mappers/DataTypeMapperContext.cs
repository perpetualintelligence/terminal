/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace OneImlx.Terminal.Commands.Mappers
{
    /// <summary>
    /// The <see cref="IValue"/> data-type mapper context.
    /// </summary>
    public sealed class DataTypeMapperContext<TValue> where TValue : IValue
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public DataTypeMapperContext(TValue value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// The value to map.
        /// </summary>
        public TValue Value { get; set; }
    }
}