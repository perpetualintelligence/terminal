/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Mappers
{
    /// <summary>
    /// The default <c>oneimlx</c> argument data type extractor.
    /// </summary>
    /// <remarks>
    /// The default syntax format is either <c>-{arg}={value}</c> for a key-value pair or <c>-{arg}</c> for a boolean argument.
    /// </remarks>
    public class DataAnnotationsDataTypeMapper : IArgumentDataTypeMapper
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public DataAnnotationsDataTypeMapper(CliOptions options, ILogger<DataAnnotationsDataTypeMapper> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public Task<Type> MapAsync(Argument context)
        {
            switch (context.DataType)
            {
                case DataType.CreditCard: return Task.FromResult(typeof(string));
                case DataType.Currency: return Task.FromResult(typeof(string));
                case DataType.Date: return Task.FromResult(typeof(DateOnly));
                case DataType.DateTime: return Task.FromResult(typeof(DateTime));
                case DataType.Duration: return Task.FromResult(typeof(TimeOnly));
                case DataType.EmailAddress: return Task.FromResult(typeof(string));
                case DataType.Html: return Task.FromResult(typeof(DateOnly));
                case DataType.ImageUrl: return Task.FromResult(typeof(Uri));
                case DataType.MultilineText: return Task.FromResult(typeof(string));
                case DataType.Password: return Task.FromResult(typeof(string));
                case DataType.PhoneNumber: return Task.FromResult(typeof(string));
                case DataType.PostalCode: return Task.FromResult(typeof(string));
                case DataType.Text: return Task.FromResult(typeof(string));
                case DataType.Time: return Task.FromResult(typeof(TimeOnly));
                case DataType.Upload: return Task.FromResult(typeof(string));
                case DataType.Url: return Task.FromResult(typeof(Uri));
                case DataType.Custom:
                    {
                        switch (context.CustomDataType)
                        {
                            case nameof(Boolean): return Task.FromResult(typeof(bool));
                            case nameof(String): return Task.FromResult(typeof(string));
                            case nameof(Int16): return Task.FromResult(typeof(short));
                            case nameof(Int32): return Task.FromResult(typeof(int));
                            case nameof(Int64): return Task.FromResult(typeof(long));
                            case nameof(Single): return Task.FromResult(typeof(float));
                            case nameof(Double): return Task.FromResult(typeof(double));
                            default:
                                {
                                    throw new NotSupportedException();
                                }
                        }
                    }
                default:
                    {
                        throw new NotSupportedException();
                    }
            }
        }

        private readonly ILogger<DataAnnotationsDataTypeMapper> logger;
        private readonly CliOptions options;
    }
}
