/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Extractors
{
    /// <summary>
    /// The default <c>oneimlx</c> argument syntax checker.
    /// </summary>
    /// <remarks>
    /// The default syntax format is either <c>-{arg}={value}</c> for a key-value pair or <c>-{arg}</c> for a boolean argument.
    /// </remarks>
    public class DefaultArgumentDataTypeExtractor : IArgumentsDataTypeExtractor
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">The logger.</param>
        public DefaultArgumentDataTypeExtractor(CliOptions options, ILogger<DefaultArgumentDataTypeExtractor> logger)
        {
            this.options = options;
            this.logger = logger;
        }

        public Task<Type> ExtractAsync(Argument context)
        {
            throw new NotImplementedException();
        }

        private readonly ILogger<DefaultArgumentDataTypeExtractor> logger;
        private readonly CliOptions options;
    }
}
