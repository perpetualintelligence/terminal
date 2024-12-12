/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System.Threading.Tasks;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Commands.Handlers;

namespace OneImlx.Terminal.Commands.Routers.Mocks
{
    internal class MockCommandHandlerInner : ICommandHandler
    {
        public MockCommandHandlerInner()
        {
        }

        public bool Called { get; set; }

        public CommandRouterContext? PassedContext { get; internal set; }

        public bool IsExplicitError { get; internal set; }

        public Task HandleCommandAsync(CommandRouterContext context)
        {
            Called = true;

            PassedContext = context;

            if (IsExplicitError)
            {
                throw new TerminalException("test_handler_error", "test_handler_error_desc");
            }

            context.Result = new CommandRouterResult();
            return Task.CompletedTask;
        }
    }
}
