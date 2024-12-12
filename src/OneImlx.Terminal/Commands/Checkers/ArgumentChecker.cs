/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OneImlx.Terminal.Commands.Mappers;
using OneImlx.Terminal.Configuration.Options;

namespace OneImlx.Terminal.Commands.Checkers
{
    /// <summary>
    /// The default argument checker.
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
        public ArgumentChecker(IDataTypeMapper<Argument> mapper, TerminalOptions options)
        {
            this.mapper = mapper;
            this.options = options;
        }

        /// <inheritdoc/>
        public async Task<ArgumentCheckerResult> CheckArgumentAsync(Argument argument)
        {
            // Check for null argument value
            if (argument.Value == null)
            {
                throw new TerminalException(TerminalErrors.InvalidOption, "The argument value cannot be null. argument={0}", argument.Id);
            }

            // Check argument data type and value type
            DataTypeMapperResult mapperResult = await mapper.MapToTypeAsync(new DataTypeMapperContext<Argument>(argument));

            // Check whether we need to check type
            if (options.Checker.StrictValueType.GetValueOrDefault())
            {
                // Check value compatibility
                await StrictTypeCheckingAsync(argument, mapperResult);
            }

            // Check value
            if (argument.Descriptor.ValueCheckers != null)
            {
                foreach (IValueChecker<Argument> valueChecker in argument.Descriptor.ValueCheckers)
                {
                    try
                    {
                        await valueChecker.CheckValueAsync(argument);
                    }
                    catch (Exception ex)
                    {
                        throw new TerminalException(TerminalErrors.InvalidOption, "The argument value is not valid. argument={0} value={1} info={2}", argument.Id, argument.Value, ex.Message);
                    }
                }
            }

            return new ArgumentCheckerResult(mapperResult.MappedType);
        }

        /// <summary>
        /// Checks the argument value compatibility.
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="mapperResult"></param>
        /// <returns></returns>
        protected Task<OptionCheckerResult> StrictTypeCheckingAsync(Argument argument, DataTypeMapperResult mapperResult)
        {
            // Ensure strict value compatibility
            try
            {
                argument.ChangeValueType(mapperResult.MappedType);
            }
            catch
            {
                // Meaningful error instead of format exception
                throw new TerminalException(TerminalErrors.InvalidArgument, "The argument value does not match the mapped type. argument={0} type={1} data_type={2} value_type={3} value={4}", argument.Id, mapperResult.MappedType, argument.DataType, argument.Value.GetType().Name, argument.Value);
            }

            return Task.FromResult(new OptionCheckerResult(mapperResult.MappedType));
        }

        private readonly IDataTypeMapper<Argument> mapper;
        private readonly TerminalOptions options;
    }
}
