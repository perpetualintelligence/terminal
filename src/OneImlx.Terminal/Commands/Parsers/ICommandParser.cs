/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;

namespace OneImlx.Terminal.Commands.Parsers
{
    /// <summary>
    /// An abstraction to parse the raw command string and extract <see cref="Command"/>.
    /// </summary>
    public interface ICommandParser
    {
        /// <summary>
        /// Extracts <see cref="Command"/> asynchronously.
        /// </summary>
        /// <param name="context">The option extraction context.</param>
        public Task ParseCommandAsync(CommandRouterContext context);
    }
}
