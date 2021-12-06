/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Commands.Checkers;
using PerpetualIntelligence.Cli.Commands.Mappers;
using PerpetualIntelligence.Cli.Configuration.Options;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Checkers
{
    /// <summary>
    /// The default <c>oneimlx</c> argument data type extractor.
    /// </summary>
    /// <remarks>
    /// The default syntax format is either <c>-{arg}={value}</c> for a key-value pair or <c>-{arg}</c> for a boolean argument.
    /// </remarks>
    public class DataAnnotationsDataTypeChecker : IArgumentDataTypeChecker
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="mapper">The argument data-type mapper.</param>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public DataAnnotationsDataTypeChecker(IArgumentDataTypeMapper mapper, CliOptions options, ILogger<DataAnnotationsDataTypeChecker> logger)
        {
            this.mapper = mapper;
            this.options = options;
            this.logger = logger;
        }

        public async Task<ArgumentDataTypeCheckerResult> CheckAsync(ArgumentDataTypeCheckerContext context)
        {
            if (context.Argument.Value == null)
            {
                // TODO retirn error
                return new ArgumentDataTypeCheckerResult();
            }

            Type dataType = await mapper.MapAsync(context.Argument);
            if (!dataType.IsAssignableFrom(context.Argument.Value.GetType()))
            {
                return new ArgumentDataTypeCheckerResult();
            }

            return new ArgumentDataTypeCheckerResult();
        }

        private readonly ILogger<DataAnnotationsDataTypeChecker> logger;
        private readonly IArgumentDataTypeMapper mapper;
        private readonly CliOptions options;
    }
}
