/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Shared.Exceptions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Mappers
{
    /// <summary>
    /// The argument data type mapper using <see cref="System.ComponentModel.DataAnnotations"/>.
    /// </summary>
    public class DataAnnotationsArgumentDataTypeMapper : IArgumentDataTypeMapper
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public DataAnnotationsArgumentDataTypeMapper(CliOptions options, ILogger<DataAnnotationsArgumentDataTypeMapper> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public Task<ArgumentDataTypeMapperResult> MapAsync(ArgumentDataTypeMapperContext context)
        {
            if (context.Argument.DataType == DataType.Custom && string.IsNullOrWhiteSpace(context.Argument.CustomDataType))
            {
                throw new ErrorException(Errors.InvalidArgument, "The argument custom data type is null or whitespace. argument={0}", context.Argument.Id);
            }

            switch (context.Argument.DataType)
            {
                case DataType.CreditCard: return Valid(typeof(string));
                case DataType.Currency: return Valid(typeof(string));
                case DataType.Date: return Valid(typeof(DateTime));
                case DataType.DateTime: return Valid(typeof(DateTime));
                case DataType.Duration: return Valid(typeof(TimeSpan));
                case DataType.EmailAddress: return Valid(typeof(string));
                case DataType.Html: return Valid(typeof(string));
                case DataType.ImageUrl: return Valid(typeof(Uri));
                case DataType.MultilineText: return Valid(typeof(string));
                case DataType.Password: return Valid(typeof(string));
                case DataType.PhoneNumber: return Valid(typeof(string));
                case DataType.PostalCode: return Valid(typeof(string));
                case DataType.Text: return Valid(typeof(string));
                case DataType.Time: return Valid(typeof(DateTime));
                case DataType.Upload: return Valid(typeof(string));
                case DataType.Url: return Valid(typeof(Uri));
                case DataType.Custom:
                    {
                        switch (context.Argument.CustomDataType)
                        {
                            // The system defined custom data type don't need any type validation as they are validated
                            // explicitly by the checker.
                            case nameof(Boolean): return Valid(typeof(bool));
                            case nameof(String): return Valid(typeof(string));
                            case nameof(Int16): return Valid(typeof(short));
                            case nameof(UInt16): return Valid(typeof(ushort));
                            case nameof(Int32): return Valid(typeof(int));
                            case nameof(UInt32): return Valid(typeof(uint));
                            case nameof(Int64): return Valid(typeof(long));
                            case nameof(UInt64): return Valid(typeof(ulong));
                            case nameof(Single): return Valid(typeof(float));
                            case nameof(Double): return Valid(typeof(double));
                            default:
                                {
                                    throw new ErrorException(Errors.UnsupportedArgument, "The argument custom data type is not supported. argument={0} custom_data_type={1}", context.Argument.Id, context.Argument.CustomDataType);
                                }
                        }
                    }
                default:
                    {
                        throw new ErrorException(Errors.UnsupportedArgument, "The argument data type is not supported. argument={0} data_type={1}", context.Argument.Id, context.Argument.DataType);
                    }
            }
        }

        private Task<ArgumentDataTypeMapperResult> Valid(Type type)
        {
            return Task.FromResult(new ArgumentDataTypeMapperResult(type));
        }

        private readonly ILogger<DataAnnotationsArgumentDataTypeMapper> logger;
        private readonly CliOptions options;
    }
}
