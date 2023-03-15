/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;

namespace PerpetualIntelligence.Cli.Commands.Mappers
{
    /// <summary>
    /// The option data-type mapper context.
    /// </summary>
    public sealed class OptionDataTypeMapperContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public OptionDataTypeMapperContext(Option option)
        {
            Argument = option ?? throw new ArgumentNullException(nameof(option));
        }

        /// <summary>
        /// The option to map.
        /// </summary>
        public Option Argument { get; set; }
    }
}
