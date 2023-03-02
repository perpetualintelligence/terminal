/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Handlers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockCommandHandler : ICommandHandler
    {
        public bool Called { get; set; }

        public Task<CommandHandlerResult> HandleAsync(CommandHandlerContext context)
        {
            Called = true;
            return Task.FromResult(new CommandHandlerResult(new Commands.Runners.CommandRunnerResult(), new Commands.Checkers.CommandCheckerResult()));
        }
    }
}