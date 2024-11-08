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
        /// <param name="commandRequestParser">The command request parser.</param>
        /// <param name="logger">The logger.</param>
        public CommandParser(ICommandRequestParser commandRequestParser, ILogger<CommandParser> logger)
        {
            this.commandRequestParser = commandRequestParser ?? throw new ArgumentNullException(nameof(commandRequestParser));
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<CommandParserResult> ParseCommandAsync(CommandParserContext context)
        {
            logger.LogDebug("Parse command. request={0}", context.Request.Id);
            ParsedCommand parsedCommand = await commandRequestParser.ParseRequestAsync(context.Request);
            return new CommandParserResult(parsedCommand);
        }

        private readonly ICommandRequestParser commandRequestParser;
        private readonly ILogger<CommandParser> logger;
    }
}