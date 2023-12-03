/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Parsers
{
    /// <summary>
    /// An abstraction to extract a <see cref="Command"/>.
    /// </summary>
    public interface ICommandParser
    {
        /// <summary>
        /// Extracts <see cref="Command"/> asynchronously.
        /// </summary>
        /// <param name="context">The option extraction context.</param>
        /// <returns>The <see cref="CommandParserResult"/> instance.</returns>
        public Task<CommandParserResult> ParseCommandAsync(CommandParserContext context);
    }
}