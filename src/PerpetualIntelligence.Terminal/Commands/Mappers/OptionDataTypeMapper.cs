﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Terminal.Configuration.Options;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Mappers
{
    /// <summary>
    /// The default option data type mapper.
    /// </summary>
    public sealed class OptionDataTypeMapper : IOptionDataTypeMapper
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public OptionDataTypeMapper(TerminalOptions options, ILogger<OptionDataTypeMapper> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public Task<OptionDataTypeMapperResult> MapAsync(OptionDataTypeMapperContext context)
        {
            if (string.IsNullOrWhiteSpace(context.Option.DataType))
            {
                throw new ErrorException(TerminalErrors.InvalidOption, "The option data type cannot be null or whitespace. option={0}", context.Option.Id);
            }

            switch (context.Option.DataType)
            {
                // The system defined custom data type don't need any type validation as they are validated
                // explicitly by the checker.
                case nameof(Boolean): return MapperResultAsync(typeof(bool));
                case nameof(String): return MapperResultAsync(typeof(string));
                case nameof(Int16): return MapperResultAsync(typeof(short));
                case nameof(UInt16): return MapperResultAsync(typeof(ushort));
                case nameof(Int32): return MapperResultAsync(typeof(int));
                case nameof(UInt32): return MapperResultAsync(typeof(uint));
                case nameof(Int64): return MapperResultAsync(typeof(long));
                case nameof(UInt64): return MapperResultAsync(typeof(ulong));
                case nameof(Single): return MapperResultAsync(typeof(float));
                case nameof(Double): return MapperResultAsync(typeof(double));
                default:
                    {
                        throw new ErrorException(TerminalErrors.UnsupportedOption, "The option data type is not supported. option={0} data_type={1}", context.Option.Id, context.Option.DataType);
                    }
            }
        }

        private Task<OptionDataTypeMapperResult> MapperResultAsync(Type type)
        {
            return Task.FromResult(new OptionDataTypeMapperResult(type));
        }

        private readonly ILogger<OptionDataTypeMapper> logger;
        private readonly TerminalOptions options;
    }
}