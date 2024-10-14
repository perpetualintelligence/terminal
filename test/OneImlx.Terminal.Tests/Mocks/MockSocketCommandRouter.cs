/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Shared.Infrastructure;
using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Commands.Routers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OneImlx.Terminal.Mocks
{
    public class MockSocketCommandRouter : ICommandRouter
    {
        public MockSocketCommandRouter(int? routeDelay = null, CancellationTokenSource? cancelOnRouteCalled = null, Exception? exception = null, Error? explicitError = null)
        {
            this.routeDelay = routeDelay;
            this.cancelOnRouteCalled = cancelOnRouteCalled;
            this.exception = exception;
            this.explicitError = explicitError;

            RawCommandStrings = [];
        }

        public bool FindCalled { get; set; }

        public List<string> RawCommandStrings { get; }

        public bool RouteCalled { get; set; }

        //This is used in the context of singleton Router
        public int RouteCounter { get; set; }

        public async Task<CommandRouterResult> RouteCommandAsync(CommandRouterContext context)
        {
            // Stats
            RouteCalled = true;
            RawCommandStrings.Add(context.Route.Raw);
            RouteCounter += 1;

            // Add delay
            if (routeDelay != null)
            {
                await Task.Delay(routeDelay.Value);
            }

            // Cancel on route first
            if (cancelOnRouteCalled != null)
            {
                cancelOnRouteCalled.Cancel();
            }

            // Raise exception
            if (exception != null)
            {
                throw exception;
            }

            // Raise explicit error
            if (explicitError != null)
            {
                throw new TerminalException(explicitError);
            }

            return new CommandRouterResult(new CommandHandlerResult(new Commands.Checkers.CommandCheckerResult(), new Commands.Runners.CommandRunnerResult()), context.Route);
        }

        private readonly CancellationTokenSource? cancelOnRouteCalled;
        private readonly Exception? exception;
        private readonly Error? explicitError;
        private readonly int? routeDelay;
    }
}
