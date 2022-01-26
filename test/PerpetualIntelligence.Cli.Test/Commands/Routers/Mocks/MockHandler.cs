/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Shared.Exceptions;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Routers.Mocks
{
    internal class MockHandler : ICommandHandler
    {
        public MockHandler()
        {
        }

        public bool Called { get; set; }

        public CommandHandlerContext? ContextCalled { get; internal set; }

        public bool IsExplicitError { get; internal set; }

        public Task<CommandHandlerResult> HandleAsync(CommandHandlerContext context)
        {
            Called = true;

            ContextCalled = context;

            if (IsExplicitError)
            {
                throw new ErrorException("test_handler_error", "test_handler_error_desc");
            }
            else
            {
                return Task.FromResult(new CommandHandlerResult());
            }
        }
    }
}
