/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using Microsoft.Extensions.Logging;
using OneImlx.Terminal.Configuration.Options;
using System;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Parsers
{
    /// <summary>
    /// The default <see cref="ICommandParser"/>.
    /// </summary>
    /// <seealso cref="ParserOptions.Separator"/>
    public class CommandParser : ICommandParser
    {
        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        /// <param name="commandRouteParser">The command route parser.</param>
        /// <param name="logger">The logger.</param>
        public CommandParser(ICommandRouteParser commandRouteParser, ILogger<CommandParser> logger)
        {
            this.commandRouteParser = commandRouteParser ?? throw new ArgumentNullException(nameof(commandRouteParser));
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<CommandParserResult> ParseCommandAsync(CommandParserContext context)
        {
            logger.LogDebug("Parse command. route={0}", context.Route.Id);
            ParsedCommand parsedCommand = await commandRouteParser.ParseRouteAsync(context.Route);
            return new CommandParserResult(parsedCommand);
        }

        private readonly ICommandRouteParser commandRouteParser;
        private readonly ILogger<CommandParser> logger;
    }
}