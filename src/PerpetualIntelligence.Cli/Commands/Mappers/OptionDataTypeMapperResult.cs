/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Commands.Mappers
{
    /// <summary>
    /// The option data-type mapper result.
    /// </summary>
    public sealed class OptionDataTypeMapperResult
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="mappedType">The mapped type.</param>
        /// <exception cref="ArgumentNullException">Null mapped type.</exception>
        public OptionDataTypeMapperResult(Type mappedType)
        {
            MappedType = mappedType ?? throw new ArgumentNullException(nameof(mappedType));
        }

        /// <summary>
        /// The mapped system type.
        /// </summary>
        public Type MappedType { get; }
    }
}