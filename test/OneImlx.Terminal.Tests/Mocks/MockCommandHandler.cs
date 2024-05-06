/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Terminal.Commands.Handlers;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Mocks
{
    public class MockCommandHandler : ICommandHandler
    {
        public bool Called { get; set; }

        public Task<CommandHandlerResult> HandleCommandAsync(CommandHandlerContext context)
        {
            Called = true;
            return Task.FromResult(new CommandHandlerResult(new Commands.Checkers.CommandCheckerResult(), new Commands.Runners.CommandRunnerResult()));
        }
    }
}