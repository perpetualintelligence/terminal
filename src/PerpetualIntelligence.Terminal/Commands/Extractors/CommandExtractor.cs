/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using PerpetualIntelligence.Terminal.Configuration.Options;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Extractors
{
    /// <summary>
    /// The default <see cref="ICommandExtractor"/>.
    /// </summary>
    /// <seealso cref="ExtractorOptions.Separator"/>
    public class CommandExtractor : ICommandExtractor
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="commandRouteParser">The command route parser.</param>
        /// <param name="logger">The logger.</param>
        public CommandExtractor(ICommandRouteParser commandRouteParser, ILogger<CommandExtractor> logger)
        {
            this.commandRouteParser = commandRouteParser ?? throw new ArgumentNullException(nameof(commandRouteParser));
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<CommandExtractorResult> ExtractCommandAsync(CommandExtractorContext context)
        {
            logger.LogDebug("Extract route. route={0}", context.Route.Id);
            ParsedCommand parsedCommand = await commandRouteParser.ParseRouteAsync(context.Route);
            return new CommandExtractorResult(parsedCommand);
        }

        private readonly ICommandRouteParser commandRouteParser;
        private readonly ILogger<CommandExtractor> logger;
    }
}