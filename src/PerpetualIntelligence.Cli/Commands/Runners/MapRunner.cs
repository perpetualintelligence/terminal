/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Cli.Configuration.Options;

namespace PerpetualIntelligence.Cli.Commands.RequestHandlers
{
    /// <summary>
    /// The <c>map</c> command request handler.
    /// </summary>
    public class MapRunner : CommandRunner
    {
        /// <summary>
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public MapRunner(CliOptions options, ILogger<CommandRunner> logger) : base(options, logger)
        {
        }
    }
}
