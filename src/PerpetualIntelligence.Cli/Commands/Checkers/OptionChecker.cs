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
    /// The default option checker.
    /// </summary>
    /// <remarks>
    /// The <see cref="OptionChecker"/> uses the <see cref="ValidationAttribute"/> to check an option value.
    /// </remarks>
    public class OptionChecker : IOptionChecker
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="mapper">The option data-type mapper.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public OptionChecker(IArgumentDataTypeMapper mapper, CliOptions options, ILogger<OptionChecker> logger)
        {
            this.mapper = mapper;
            this.options = options;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<OptionCheckerResult> CheckAsync(OptionCheckerContext context)
        {
            // Check for null option value
            if (context.Argument.Value == null)
            {
                throw new ErrorException(Errors.InvalidArgument, "The option value cannot be null. option={0}", context.Argument.Id);
            }

            // Check option data type and value type
            ArgumentDataTypeMapperResult mapperResult = await mapper.MapAsync(new ArgumentDataTypeMapperContext(context.Argument));

            // Check whether we need to check type
            if (options.Checker.StrictArgumentValueType.GetValueOrDefault())
            {
                // Check value compatibility
                return await StrictTypeCheckingAsync(context, mapperResult);
            }

            return new OptionCheckerResult(mapperResult.MappedType);
        }

        /// <summary>
        /// Checks the option value compatibility.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="mapperResult"></param>
        /// <returns></returns>
        protected Task<OptionCheckerResult> StrictTypeCheckingAsync(OptionCheckerContext context, ArgumentDataTypeMapperResult mapperResult)
        {
            // Ensure strict value compatibility
            try
            {
                context.Argument.ChangeValueType(mapperResult.MappedType);
            }
            catch
            {
                // Meaningful error instead of format exception
                throw new ErrorException(Errors.InvalidArgument, "The option value does not match the mapped type. option={0} type={1} data_type={2} value_type={3} value={4}", context.Argument.Id, mapperResult.MappedType, context.Argument.DataType, context.Argument.Value.GetType().Name, context.Argument.Value);
            }

            if (context.ArgumentDescriptor.ValueCheckers != null)
            {
                foreach (IOptionValueChecker valueChecker in context.ArgumentDescriptor.ValueCheckers)
                {
                    try
                    {
                        valueChecker.CheckAsync(context.Argument);
                    }
                    catch (Exception ex)
                    {
                        throw new ErrorException(Errors.InvalidArgument, "The option value is not valid. option={0} value={1} info={2}", context.Argument.Id, context.Argument.Value, ex.Message);
                    }
                }
            }

            return Task.FromResult(new OptionCheckerResult(mapperResult.MappedType));
        }

        private readonly ILogger<OptionChecker> logger;
        private readonly IArgumentDataTypeMapper mapper;
        private readonly CliOptions options;
    }
}