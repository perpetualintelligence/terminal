/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Infrastructure;
using System;

namespace PerpetualIntelligence.Cli.Commands.Mappers
{
    /// <summary>
    /// The argument data-type mapper result.
    /// </summary>
    public class ArgumentDataTypeMapperResult : OneImlxResult
    {
        /// <summary>
        /// The mapped system type.
        /// </summary>
        public Type? MappedType { get; set; }
    }
}
