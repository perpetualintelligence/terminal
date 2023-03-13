/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Commands.Mappers
{
    /// <summary>
    /// The argument data-type mapper context.
    /// </summary>
    public class ArgumentDataTypeMapperContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ArgumentDataTypeMapperContext(Option argument)
        {
            Argument = argument ?? throw new ArgumentNullException(nameof(argument));
        }

        /// <summary>
        /// The argument to map.
        /// </summary>
        public Option Argument { get; set; }
    }
}
