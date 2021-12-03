/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Shared.Infrastructure;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Routers.Mocks
{
    internal class MockHandler : ICommandHandler
    {
        public MockHandler()
        {
        }

        public bool Called { get; set; }

        public bool IsExplicitError { get; internal set; }

        public CommandHandlerContext ContextCalled { get; internal set; }

        public Task<CommandHandlerResult> HandleAsync(CommandHandlerContext context)
        {
            Called = true;

            ContextCalled = context;

            if (IsExplicitError)
            {
                return Task.FromResult(OneImlxResult.NewError<CommandHandlerResult>("test_handler_error", "test_handler_error_desc"));
            }
            else
            {
                return Task.FromResult(new CommandHandlerResult());
            }
        }
    }
}
