/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OneImlx.Terminal.Configuration.Options;

namespace OneImlx.Terminal.Commands.Checkers
{
    /// <summary>
    /// The default option checker.
    /// </summary>
    /// <remarks>The <see cref="OptionChecker"/> uses the <see cref="ValidationAttribute"/> to check an option value.</remarks>
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
        public async Task<OptionCheckerResult> CheckOptionAsync(Option option)
        {
            // Check for null option value
            if (option.Value == null)
            {
                throw new TerminalException(TerminalErrors.InvalidOption, "The option value cannot be null. option={0}", option.Id);
            }

            // Check option data type and value type
            DataTypeMapperResult mapperResult = await mapper.MapToTypeAsync(option);

            // Check whether we need to check type
            if (options.Checker.StrictValueType)
            {
                // Check value compatibility
                await StrictTypeCheckingAsync(option, mapperResult);
            }

            // Check value
            if (option.Descriptor.ValueCheckers != null)
            {
                foreach (IValueChecker<Option> valueChecker in option.Descriptor.ValueCheckers)
                {
                    try
                    {
                        await valueChecker.CheckValueAsync(option);
                    }
                    catch (Exception ex)
                    {
                        throw new TerminalException(TerminalErrors.InvalidOption, "The option value is not valid. option={0} value={1} info={2}", option.Id, option.Value, ex.Message);
                    }
                }
            }

            return new OptionCheckerResult(mapperResult.MappedType);
        }

        /// <summary>
        /// Checks the option value compatibility.
        /// </summary>
        /// <param name="option"></param>
        /// <param name="mapperResult"></param>
        /// <returns></returns>
        protected Task<OptionCheckerResult> StrictTypeCheckingAsync(Option option, DataTypeMapperResult mapperResult)
        {
            // Ensure strict value compatibility
            try
            {
                option.ChangeValueType(mapperResult.MappedType);
            }
            catch
            {
                // Meaningful error instead of format exception
                throw new TerminalException(TerminalErrors.InvalidOption, "The option value does not match the mapped type. option={0} type={1} data_type={2} value_type={3} value={4}", option.Id, mapperResult.MappedType, option.DataType, option.Value.GetType().Name, option.Value);
            }

            return Task.FromResult(new OptionCheckerResult(mapperResult.MappedType));
        }

        private readonly IDataTypeMapper<Option> mapper;
        private readonly TerminalOptions options;
    }
}
