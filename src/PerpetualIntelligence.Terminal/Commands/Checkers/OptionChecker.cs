/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Mappers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using PerpetualIntelligence.Shared.Exceptions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Checkers
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
        public OptionChecker(IOptionDataTypeMapper mapper, TerminalOptions options)
        {
            this.mapper = mapper;
            this.options = options;
        }

        /// <inheritdoc/>
        public async Task<OptionCheckerResult> CheckAsync(OptionCheckerContext context)
        {
            // Check for null option value
            if (context.Option.Value == null)
            {
                throw new ErrorException(TerminalErrors.InvalidOption, "The option value cannot be null. option={0}", context.Option.Id);
            }

            // Check option data type and value type
            OptionDataTypeMapperResult mapperResult = await mapper.MapAsync(new OptionDataTypeMapperContext(context.Option));

            // Check whether we need to check type
            if (options.Checker.StrictOptionValueType.GetValueOrDefault())
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
        protected Task<OptionCheckerResult> StrictTypeCheckingAsync(OptionCheckerContext context, OptionDataTypeMapperResult mapperResult)
        {
            // Ensure strict value compatibility
            try
            {
                context.Option.ChangeValueType(mapperResult.MappedType);
            }
            catch
            {
                // Meaningful error instead of format exception
                throw new ErrorException(TerminalErrors.InvalidOption, "The option value does not match the mapped type. option={0} type={1} data_type={2} value_type={3} value={4}", context.Option.Id, mapperResult.MappedType, context.Option.DataType, context.Option.Value.GetType().Name, context.Option.Value);
            }

            if (context.OptionDescriptor.ValueCheckers != null)
            {
                foreach (IOptionValueChecker valueChecker in context.OptionDescriptor.ValueCheckers)
                {
                    try
                    {
                        valueChecker.CheckAsync(context.Option);
                    }
                    catch (Exception ex)
                    {
                        throw new ErrorException(TerminalErrors.InvalidOption, "The option value is not valid. option={0} value={1} info={2}", context.Option.Id, context.Option.Value, ex.Message);
                    }
                }
            }

            return Task.FromResult(new OptionCheckerResult(mapperResult.MappedType));
        }

        private readonly IOptionDataTypeMapper mapper;
        private readonly TerminalOptions options;
    }
}