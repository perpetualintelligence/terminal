/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Commands.Parsers
{
    /// <summary>
    /// An abstraction to parse a command route.
    /// </summary>
    public interface ICommandRouteParser
    {
        /// <summary>
        /// Parses the command route asynchronously.
        /// </summary>
        /// <param name="commandRoute">The command route to parse.</param>
        /// <returns></returns>
        Task<ParsedCommand> ParseRouteAsync(TerminalProcessorRequest commandRoute);
    }
}