/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Shared.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;

namespace PerpetualIntelligence.Cli.Commands.Mappers
{
    /// <summary>
    /// The argument checker result.
    /// </summary>
    public class DataAnnotationMapperResult : OneImlxResult
    {
        /// <summary>
        /// The mapped system type.
        /// </summary>
        public Type? MappedSystemType { get; set; }

        /// <summary>
        /// The mapped <see cref="ValidationAttribute"/> type.
        /// </summary>
        public Type? MappedValidationAttribute { get; set; }
    }
}
