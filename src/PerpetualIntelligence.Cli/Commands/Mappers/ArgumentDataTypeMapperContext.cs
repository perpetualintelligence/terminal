/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Commands.Mappers
{
    /// <summary>
    /// The option data-type mapper context.
    /// </summary>
    public class ArgumentDataTypeMapperContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ArgumentDataTypeMapperContext(Option option)
        {
            Argument = option ?? throw new ArgumentNullException(nameof(option));
        }

        /// <summary>
        /// The option to map.
        /// </summary>
        public Option Argument { get; set; }
    }
}
