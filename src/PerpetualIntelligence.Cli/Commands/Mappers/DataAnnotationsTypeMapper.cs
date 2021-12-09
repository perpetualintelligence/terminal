/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Mappers
{
    /// <summary>
    /// The <c>cli</c> data type mapper for <see cref="DataType"/>.
    /// </summary>
    public class DataAnnotationsTypeMapper : IArgumentMapper
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public DataAnnotationsTypeMapper(CliOptions options, ILogger<DataAnnotationsTypeMapper> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public Task<DataAnnotationsMapperTypeResult> MapAsync(DataAnnotationsMapperTypeContext context)
        {
            if (context.Argument.DataType == DataType.Custom && string.IsNullOrWhiteSpace(context.Argument.CustomDataType))
            {
                return Task.FromResult(OneImlxResult.NewError<DataAnnotationsMapperTypeResult>(Errors.InvalidArgument, logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument custom data type is null or whitespace. argument={0}", context.Argument.Name)));
            }

            switch (context.Argument.DataType)
            {
                case DataType.CreditCard: return Task.FromResult(Valid(typeof(string)));
                case DataType.Currency: return Task.FromResult(Valid(typeof(string)));
                case DataType.Date: return Task.FromResult(Valid(typeof(DateTime)));
                case DataType.DateTime: return Task.FromResult(Valid(typeof(DateTime)));
                case DataType.Duration: return Task.FromResult(Valid(typeof(TimeSpan)));
                case DataType.EmailAddress: return Task.FromResult(Valid(typeof(string)));
                case DataType.Html: return Task.FromResult(Valid(typeof(string)));
                case DataType.ImageUrl: return Task.FromResult(Valid(typeof(Uri)));
                case DataType.MultilineText: return Task.FromResult(Valid(typeof(string)));
                case DataType.Password: return Task.FromResult(Valid(typeof(string)));
                case DataType.PhoneNumber: return Task.FromResult(Valid(typeof(string)));
                case DataType.PostalCode: return Task.FromResult(Valid(typeof(string)));
                case DataType.Text: return Task.FromResult(Valid(typeof(string)));
                case DataType.Time: return Task.FromResult(Valid(typeof(DateTime)));
                case DataType.Upload: return Task.FromResult(Valid(typeof(string)));
                case DataType.Url: return Task.FromResult(Valid(typeof(Uri)));
                case DataType.Custom:
                    {
                        switch (context.Argument.CustomDataType)
                        {
                            // The system defined custom data type don't need any type validation as they are validated
                            // explicitly by the checker.
                            case nameof(Boolean): return Task.FromResult(Valid(typeof(bool)));
                            case nameof(String): return Task.FromResult(Valid(typeof(string)));
                            case nameof(Int16): return Task.FromResult(Valid(typeof(short)));
                            case nameof(UInt16): return Task.FromResult(Valid(typeof(ushort)));
                            case nameof(Int32): return Task.FromResult(Valid(typeof(int)));
                            case nameof(UInt32): return Task.FromResult(Valid(typeof(uint)));
                            case nameof(Int64): return Task.FromResult(Valid(typeof(long)));
                            case nameof(UInt64): return Task.FromResult(Valid(typeof(ulong)));
                            case nameof(Single): return Task.FromResult(Valid(typeof(float)));
                            case nameof(Double): return Task.FromResult(Valid(typeof(double)));
                            default:
                                {
                                    return Task.FromResult(OneImlxResult.NewError<DataAnnotationsMapperTypeResult>(Errors.UnsupportedArgument, logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument custom data type is not supported. argument={0} custom_data_type={1}", context.Argument.Name, context.Argument.CustomDataType)));
                                }
                        }
                    }
                default:
                    {
                        return Task.FromResult(OneImlxResult.NewError<DataAnnotationsMapperTypeResult>(Errors.UnsupportedArgument, logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument data type is not supported. argument={0} data_type={1}", context.Argument.Name, context.Argument.DataType)));
                    }
            }
        }

        private DataAnnotationsMapperTypeResult Valid(Type type)
        {
            return new DataAnnotationsMapperTypeResult() { MappedType = type };
        }

        private readonly ILogger<DataAnnotationsTypeMapper> logger;
        private readonly CliOptions options;
    }
}
