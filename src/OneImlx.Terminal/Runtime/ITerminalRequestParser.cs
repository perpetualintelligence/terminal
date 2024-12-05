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
    /// An abstraction to parse a <see cref="TerminalRequest"/>.
    /// </summary>
    public interface ITerminalRequestParser
    {
        /// <summary>
        /// Parses the terminal request asynchronously.
        /// </summary>
        /// <param name="request">The terminal request to parse.</param>
        /// <returns></returns>
        Task<TerminalParsedRequest> ParseRequestAsync(TerminalRequest request);
    }
}
