/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Parsers;
using OneImlx.Terminal.Runtime;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Mocks
{
    internal class MockCommandRouteParser : ICommandRouteParser
    {
        public TerminalProcessorRequest PassedCommandRoute { get; private set; } = null!;

        public bool Called { get; private set; }

        public Task<ParsedCommand> ParseRouteAsync(TerminalProcessorRequest commandRoute)
        {
            PassedCommandRoute = commandRoute;
            Called = true;
            return Task.FromResult(new ParsedCommand(commandRoute, new Command(new CommandDescriptor("id", "name", "description", CommandType.SubCommand, CommandFlags.None)), Root.Default()));
        }
    }
}