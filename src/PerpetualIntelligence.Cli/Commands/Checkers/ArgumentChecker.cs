/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands.Mappers;
using PerpetualIntelligence.Cli.Configuration.Options;
using PerpetualIntelligence.Shared.Exceptions;
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
                throw new ErrorException(Errors.InvalidArgument, "The argument value cannot be null. argument={0}", context.Argument.Id);
            }

            // Check argument data type and value type
            ArgumentDataTypeMapperResult mapperResult = await mapper.MapAsync(new ArgumentDataTypeMapperContext(context.Argument));

            // Check whether we need to check type
            if (options.Checker.StrictArgumentValueType.GetValueOrDefault())
            {
                // Check value compatibility
                return await StrictTypeCheckingAsync(context, mapperResult);
            }

            return new ArgumentCheckerResult(mapperResult.MappedType);
        }

        /// <summary>
        /// Checks the argument value compatibility.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="mapperResult"></param>
        /// <returns></returns>
        protected Task<ArgumentCheckerResult> StrictTypeCheckingAsync(ArgumentCheckerContext context, ArgumentDataTypeMapperResult mapperResult)
        {
            // Ensure strict value compatibility
            try
            {
                context.Argument.ChangeValueType(mapperResult.MappedType);
            }
            catch
            {
                // Meaningful error instead of format exception
                throw new ErrorException(Errors.InvalidArgument, "The argument value does not match the mapped type. argument={0} type={1} data_type={2} value_type={3} value={4}", context.Argument.Id, mapperResult.MappedType, context.Argument.DataType, context.Argument.Value.GetType().Name, context.Argument.Value);
            }

            if (context.ArgumentDescriptor.ValidationAttributes != null)
            {
                foreach (ValidationAttribute vAttr in context.ArgumentDescriptor.ValidationAttributes)
                {
                    try
                    {
                        ValidationContext validationContext = new(context.Argument);
                        vAttr.Validate(context.Argument.Value, validationContext);
                    }
                    catch (Exception ex)
                    {
                        throw new ErrorException(Errors.InvalidArgument, "The argument value is not valid. argument={0} value={1} additional_info={2}", context.Argument.Id, context.Argument.Value, ex.Message);
                    }
                }
            }

            return Task.FromResult(new ArgumentCheckerResult(mapperResult.MappedType));
        }

        private readonly ILogger<ArgumentChecker> logger;
        private readonly IArgumentDataTypeMapper mapper;
        private readonly CliOptions options;
    }
}
