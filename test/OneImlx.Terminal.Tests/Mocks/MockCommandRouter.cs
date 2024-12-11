/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OneImlx.Shared.Infrastructure;
using OneImlx.Terminal.Commands.Handlers;
using OneImlx.Terminal.Commands.Routers;

namespace OneImlx.Terminal.Mocks
{
    public class MockCommandRouter : ICommandRouter
    {
        public MockCommandRouter(int? routeDelay = null, CancellationTokenSource? cancelOnRouteCalled = null, Exception? exception = null, Error? explicitError = null)
        {
            this.routeDelay = routeDelay;
            this.cancelOnRouteCalled = cancelOnRouteCalled;
            this.exception = exception;
            this.explicitError = explicitError;
            MultipleRawString = [];
        }

        public bool FindCalled { get; set; }

        public List<string> MultipleRawString { get; set; }

        public CommandRouterContext? PassedContext { get; private set; }

        public string? RawCommandString { get; set; }

        public CommandRouterResult? ReturnedRouterResult { get; private set; }

        public bool RouteCalled { get; set; }

        //This is used in the context of singleton Router
        public int RouteCounter { get; set; }

        public async Task<CommandRouterResult> RouteCommandAsync(CommandRouterContext context)
        {
            // For testing this is a singleton router so make sure it is thread safe
            await routeLock.WaitAsync();

            // Store the context for testing
            PassedContext = context;

            try
            {
                // Your critical section code here
                RouteCalled = true;
                RawCommandString = context.Request.Raw;
                MultipleRawString.Add(context.Request.Raw);
                RouteCounter += 1;

                if (routeDelay != null)
                {
                    await Task.Delay(routeDelay.Value);
                }

                cancelOnRouteCalled?.Cancel();

                if (exception != null)
                {
                    throw exception;
                }

                if (explicitError != null)
                {
                    throw new TerminalException(explicitError);
                }

                ReturnedRouterResult = new CommandRouterResult();

                return ReturnedRouterResult;
            }
            finally
            {
                routeLock.Release(); // Release the semaphore when done
            }
        }

        private readonly CancellationTokenSource? cancelOnRouteCalled;
        private readonly Exception? exception;
        private readonly Error? explicitError;
        private readonly int? routeDelay;
        private readonly SemaphoreSlim routeLock = new(1, 1);
    }
}
