/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Commands.Parsers
{
    /// <summary>
    /// An abstraction to parse a command request.
    /// </summary>
    public interface ICommandRequestParser
    {
        /// <summary>
        /// Parses the command request asynchronously.
        /// </summary>
        /// <param name="request">The command request to parse.</param>
        /// <returns></returns>
        Task<ParsedRequest> ParseOutputAsync(TerminalRequest request);

        /// <summary>
        /// Parses the command request asynchronously.
        /// </summary>
        /// <param name="request">The command request to parse.</param>
        /// <returns></returns>
        Task<ParsedCommand> ParseRequestAsync(TerminalRequest request);
    }
}
