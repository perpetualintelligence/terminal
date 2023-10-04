/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Terminal.Commands.Mappers;
using PerpetualIntelligence.Terminal.Configuration.Options;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Checkers
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
        public async Task<ArgumentCheckerResult> CheckAsync(ArgumentCheckerContext context)
        {
            // Check for null argument value
            if (context.Argument.Value == null)
            {
                throw new TerminalException(TerminalErrors.InvalidOption, "The argument value cannot be null. argument={0}", context.Argument.Id);
            }

            // Check argument data type and value type
            DataTypeMapperResult mapperResult = await mapper.MapAsync(new DataTypeMapperContext<Argument>(context.Argument));

            // Check whether we need to check type
            if (options.Checker.StrictValueType.GetValueOrDefault())
            {
                // Check value compatibility
                await StrictTypeCheckingAsync(context, mapperResult);
            }

            // Check value
            if (context.Argument.Descriptor.ValueCheckers != null)
            {
                foreach (IValueChecker<Argument> valueChecker in context.Argument.Descriptor.ValueCheckers)
                {
                    try
                    {
                        await valueChecker.CheckAsync(context.Argument);
                    }
                    catch (Exception ex)
                    {
                        throw new TerminalException(TerminalErrors.InvalidOption, "The argument value is not valid. argument={0} value={1} info={2}", context.Argument.Id, context.Argument.Value, ex.Message);
                    }
                }
            }

            return new ArgumentCheckerResult(mapperResult.MappedType);
        }

        /// <summary>
        /// Checks the argument value compatibility.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="mapperResult"></param>
        /// <returns></returns>
        protected Task<OptionCheckerResult> StrictTypeCheckingAsync(ArgumentCheckerContext context, DataTypeMapperResult mapperResult)
        {
            // Ensure strict value compatibility
            try
            {
                context.Argument.ChangeValueType(mapperResult.MappedType);
            }
            catch
            {
                // Meaningful error instead of format exception
                throw new TerminalException(TerminalErrors.InvalidArgument, "The argument value does not match the mapped type. argument={0} type={1} data_type={2} value_type={3} value={4}", context.Argument.Id, mapperResult.MappedType, context.Argument.DataType, context.Argument.Value.GetType().Name, context.Argument.Value);
            }

            return Task.FromResult(new OptionCheckerResult(mapperResult.MappedType));
        }

        private readonly IDataTypeMapper<Argument> mapper;
        private readonly TerminalOptions options;
    }
}