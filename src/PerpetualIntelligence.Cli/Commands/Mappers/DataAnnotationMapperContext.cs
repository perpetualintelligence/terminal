/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using System;

namespace PerpetualIntelligence.Cli.Commands.Mappers
{
    /// <summary>
    /// The argument data-type checker context.
    /// </summary>
    public class DataAnnotationMapperContext
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public DataAnnotationMapperContext(Argument argument)
        {
            Argument = argument ?? throw new ArgumentNullException(nameof(argument));
        }

        /// <summary>
        /// The extracted argument to check.
        /// </summary>
        public Argument Argument { get; set; }
    }
}
