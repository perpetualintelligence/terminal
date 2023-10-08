/*
    Copyright (c) 2021 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Terminal.Commands.Handlers;
using PerpetualIntelligence.Shared.Exceptions;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Terminal.Commands.Routers.Mocks
{
    internal class MockCommandHandlerInner : ICommandHandler
    {
        public MockCommandHandlerInner()
        {
        }

        public bool Called { get; set; }

        public CommandHandlerContext? ContextCalled { get; internal set; }

        public bool IsExplicitError { get; internal set; }

        public Task<CommandHandlerResult> HandleCommandAsync(CommandHandlerContext context)
        {
            Called = true;

            ContextCalled = context;

            if (IsExplicitError)
            {
                throw new TerminalException("test_handler_error", "test_handler_error_desc");
            }
            else
            {
                return Task.FromResult(new CommandHandlerResult(new Runners.CommandRunnerResult(), new Checkers.CommandCheckerResult()));
            }
        }
    }
}