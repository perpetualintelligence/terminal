/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands.Mappers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Protocols.Cli;
using PerpetualIntelligence.Shared.Extensions;
using PerpetualIntelligence.Shared.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    /// <summary>
    /// The argument checker.
    /// </summary>
    /// <remarks>
    /// The <see cref="ArgumentChecker"/> uses the <see cref="ValidationAttribute"/> to check an argument value.
    /// </remarks>
    public class ArgumentChecker : IArgumentChecker
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="mapper">The argument data-type mapper.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public ArgumentChecker(IArgumentDataTypeMapper mapper, CliOptions options, ILogger<ArgumentChecker> logger)
        {
            this.mapper = mapper;
            this.options = options;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ArgumentCheckerResult> CheckAsync(ArgumentCheckerContext context)
        {
            // Check for null argument value
            if (context.Argument.Value == null)
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument value cannot be null. argument={0}", context.Argument.Id);
                return OneImlxResult.NewError<ArgumentCheckerResult>(Errors.InvalidArgument, errorDesc);
            }

            // Check argument data type and value type
            ArgumentDataTypeMapperResult mapperResult = await mapper.MapAsync(new ArgumentDataTypeMapperContext(context.Argument));
            if (mapperResult.IsError)
            {
                return OneImlxResult.NewError<ArgumentCheckerResult>(mapperResult);
            }

            // Check value compatibility
            return await CheckValueCompatibilityAsync(context, mapperResult);
        }

        /// <summary>
        /// Checks the argument value compatibility.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="mapperResult"></param>
        /// <returns></returns>
        protected Task<ArgumentCheckerResult> CheckValueCompatibilityAsync(ArgumentCheckerContext context, ArgumentDataTypeMapperResult mapperResult)
        {
            ArgumentCheckerResult result = new();

            // Check the system type compatibility
            if (mapperResult.MappedType != null && !mapperResult.MappedType.IsAssignableFrom(context.Argument.Value.GetType()))
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument value does not match the mapped type. argument={0} type={1} data_type={2} value_type={3} value={4}", context.Argument.Id, mapperResult.MappedType, context.Argument.DataType, context.Argument.Value.GetType().Name, context.Argument.Value);
                return Task.FromResult(OneImlxResult.NewError<ArgumentCheckerResult>(Errors.InvalidArgument, errorDesc));
            }

            if (context.ArgumentIdentity.ValidationAttributes != null)
            {
                foreach (ValidationAttribute vAttr in context.ArgumentIdentity.ValidationAttributes)
                {
                    try
                    {
                        ValidationContext validationContext = new(context.Argument);
                        vAttr.Validate(context.Argument.Value, validationContext);
                    }
                    catch (Exception ex)
                    {
                        result.AppendError(OneImlxResult.NewError<ArgumentCheckerResult>(Errors.InvalidArgument, logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument value is not valid. argument={0} value={1} additional_info={2}", context.Argument.Id, context.Argument.Value, ex.Message)));
                    }
                }
            }

            if (!result.IsError)
            {
                result.MappedType = mapperResult.MappedType;
            }

            return Task.FromResult(result);
        }

        private readonly ILogger<ArgumentChecker> logger;
        private readonly IArgumentDataTypeMapper mapper;
        private readonly CliOptions options;
    }
}
