/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Configuration.Options;

namespace OneImlx.Terminal.Commands.Checkers
{
    /// <summary>
    /// The default option data type mapper.
    /// </summary>
    public sealed class DataTypeMapper<TValue> : IDataTypeMapper<TValue> where TValue : ICommandValue
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public DataTypeMapper(TerminalOptions options, ILogger<DataTypeMapper<TValue>> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public Task<DataTypeMapperResult> MapToTypeAsync(TValue value)
        {
            if (string.IsNullOrWhiteSpace(value.DataType))
            {
                throw new TerminalException(TerminalErrors.InvalidRequest, "The value data type cannot be null or whitespace. value={0}", value.Id);
            }

            switch (value.DataType)
            {
                // The system defined custom data type don't need any type validation as they are validated explicitly
                // by the checker.
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
                case nameof(DateTime): return MapperResultAsync(typeof(DateTime));
                default:
                    {
                        throw new TerminalException(TerminalErrors.InvalidRequest, "The value data type is not supported. value={0} data_type={1}", value.Id, value.DataType);
                    }
            }
        }

        private Task<DataTypeMapperResult> MapperResultAsync(Type type)
        {
            return Task.FromResult(new DataTypeMapperResult(type));
        }

        private readonly ILogger<DataTypeMapper<TValue>> logger;
        private readonly TerminalOptions options;
    }
}
