﻿/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands.Mappers;
using OneImlx.Terminal.Configuration.Options;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Checkers
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
        public OptionChecker(IDataTypeMapper<Option> mapper, TerminalOptions options)
        {
            this.mapper = mapper;
            this.options = options;
        }

        /// <inheritdoc/>
        public async Task<OptionCheckerResult> CheckOptionAsync(OptionCheckerContext context)
        {
            // Check for null option value
            if (context.Option.Value == null)
            {
                throw new TerminalException(TerminalErrors.InvalidOption, "The option value cannot be null. option={0}", context.Option.Id);
            }

            // Check option data type and value type
            DataTypeMapperResult mapperResult = await mapper.MapToTypeAsync(new DataTypeMapperContext<Option>(context.Option));

            // Check whether we need to check type
            if (options.Checker.StrictValueType.GetValueOrDefault())
            {
                // Check value compatibility
                await StrictTypeCheckingAsync(context, mapperResult);
            }

            // Check value
            if (context.Option.Descriptor.ValueCheckers != null)
            {
                foreach (IValueChecker<Option> valueChecker in context.Option.Descriptor.ValueCheckers)
                {
                    try
                    {
                        await valueChecker.CheckValueAsync(context.Option);
                    }
                    catch (Exception ex)
                    {
                        throw new TerminalException(TerminalErrors.InvalidOption, "The option value is not valid. option={0} value={1} info={2}", context.Option.Id, context.Option.Value, ex.Message);
                    }
                }
            }

            return new OptionCheckerResult(mapperResult.MappedType);
        }

        /// <summary>
        /// Checks the option value compatibility.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="mapperResult"></param>
        /// <returns></returns>
        protected Task<OptionCheckerResult> StrictTypeCheckingAsync(OptionCheckerContext context, DataTypeMapperResult mapperResult)
        {
            // Ensure strict value compatibility
            try
            {
                context.Option.ChangeValueType(mapperResult.MappedType);
            }
            catch
            {
                // Meaningful error instead of format exception
                throw new TerminalException(TerminalErrors.InvalidOption, "The option value does not match the mapped type. option={0} type={1} data_type={2} value_type={3} value={4}", context.Option.Id, mapperResult.MappedType, context.Option.DataType, context.Option.Value.GetType().Name, context.Option.Value);
            }

            return Task.FromResult(new OptionCheckerResult(mapperResult.MappedType));
        }

        private readonly IDataTypeMapper<Option> mapper;
        private readonly TerminalOptions options;
    }
}