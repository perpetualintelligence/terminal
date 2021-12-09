/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
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
    /// The default <c>oneimlx</c> argument data type extractor.
    /// </summary>
    /// <remarks>
    /// The default syntax format is either <c>-{arg}={value}</c> for a key-value pair or <c>-{arg}</c> for a boolean argument.
    /// </remarks>
    public class ArgumentValueChecker : IArgumentValueChecker
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="mapper">The argument data-type mapper.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public ArgumentValueChecker(IArgumentMapper mapper, CliOptions options, ILogger<ArgumentValueChecker> logger)
        {
            this.mapper = mapper;
            this.options = options;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ArgumentValueCheckerResult> CheckAsync(ArgumentValueCheckerContext context)
        {
            // Check for null argument value
            if (context.Argument.Value == null)
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument value cannot be null. argument={0}", context.Argument.Name);
                return OneImlxResult.NewError<ArgumentValueCheckerResult>(Errors.InvalidArgument, errorDesc);
            }

            // Check argument data type and value type
            DataAnnotationMapperResult mapperResult = await mapper.MapAsync(new DataAnnotationMapperContext(context.Argument));
            if (mapperResult.IsError)
            {
                return OneImlxResult.NewError<ArgumentValueCheckerResult>(mapperResult);
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
        protected Task<ArgumentValueCheckerResult> CheckValueCompatibilityAsync(ArgumentValueCheckerContext context, DataAnnotationMapperResult mapperResult)
        {
            ArgumentValueCheckerResult result = new();

            // Check the system type compatibility
            if (mapperResult.MappedSystemType != null && !mapperResult.MappedSystemType.IsAssignableFrom(context.Argument.Value.GetType()))
            {
                string errorDesc = logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument value does not match the mapped type. argument={0} type={1} data_type={2} value_type={3} value={4}", context.Argument.Name, mapperResult.MappedSystemType, context.Argument.DataType, context.Argument.Value.GetType().Name, context.Argument.Value);
                return Task.FromResult(OneImlxResult.NewError<ArgumentValueCheckerResult>(Errors.InvalidArgument, errorDesc));
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
                        result.AppendError(OneImlxResult.NewError<ArgumentValueCheckerResult>(Errors.InvalidArgument, logger.FormatAndLog(LogLevel.Error, options.Logging, "The argument value is not valid. argument={0} value={1} additional_info={2}", context.Argument.Name, context.Argument.Value, ex.Message)));
                    }
                }
            }

            if (!result.IsError)
            {
                result.MappedSystemType = mapperResult.MappedSystemType;
            }

            return Task.FromResult(result);
        }

        private readonly ILogger<ArgumentValueChecker> logger;
        private readonly IArgumentMapper mapper;
        private readonly CliOptions options;
    }
}
