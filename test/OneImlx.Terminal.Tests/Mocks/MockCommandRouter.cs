/*
    Copyright © 2019-2025 Perpetual Intelligence L.L.C. All rights reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using OneImlx.Shared.Infrastructure;
using OneImlx.Terminal.Commands;
using OneImlx.Terminal.Shared;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

        public CommandContext? PassedContext { get; private set; }

        public string? RawCommandString { get; set; }

        public CommandResult? ReturnedRouterResult { get; private set; }

        public bool RouteCalled { get; set; }

        //This is used in the context of singleton Router
        public int RouteCounter { get; set; }

        public async Task<CommandResult> RouteCommandAsync(CommandContext context)
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

                ReturnedRouterResult = new CommandResult();

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
