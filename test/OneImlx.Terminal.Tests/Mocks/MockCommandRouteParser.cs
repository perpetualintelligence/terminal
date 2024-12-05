/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Runtime;

namespace OneImlx.Terminal.Mocks
{
    internal class MockCommandRouteParser : ITerminalRequestParser
    {
        public bool Called { get; private set; }

        public TerminalRequest PassedCommandRoute { get; private set; } = null!;

        public Task<ParsedRequest> ParseOutputAsync(TerminalRequest request)
        {
            throw new System.NotImplementedException();
        }

        public Task<ParsedCommand> ParseRequestAsync(TerminalRequest request)
        {
            PassedCommandRoute = request;
            Called = true;
            return Task.FromResult(new ParsedCommand(request, new Command(new CommandDescriptor("id", "name", "description", CommandType.SubCommand, CommandFlags.None)), Root.Default()));
        }
    }
}
