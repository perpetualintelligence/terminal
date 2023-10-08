/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

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
        public CommandExtractor(ICommandRouteParser commandRouteParser)
        {
            this.commandRouteParser = commandRouteParser ?? throw new ArgumentNullException(nameof(commandRouteParser));
        }

        /// <inheritdoc/>
        public async Task<CommandExtractorResult> ExtractCommandAsync(CommandExtractorContext context)
        {
            ParsedCommand parsedCommand = await commandRouteParser.ParseRouteAsync(context.Route);
            return new CommandExtractorResult(parsedCommand);
        }

        private readonly ICommandRouteParser commandRouteParser;
    }
}