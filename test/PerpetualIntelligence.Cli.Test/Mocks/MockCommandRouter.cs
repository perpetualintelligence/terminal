/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Shared.Exceptions;
using PerpetualIntelligence.Shared.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockCommandRouter : ICommandRouter
    {
        public MockCommandRouter(int? routeDelay = null, CancellationTokenSource? cancelOnRouteCalled = null, Exception? exception = null, Error? explicitError = null)
        {
            this.routeDelay = routeDelay;
            this.cancelOnRouteCalled = cancelOnRouteCalled;
            this.exception = exception;
            this.explicitError = explicitError;
        }

        public string? CommandString { get; set; }

        public bool FindCalled { get; set; }

        public bool RouteCalled { get; set; }

        //This is used in the context of singleton Router
        public int RouteCounter { get; set; }

        public async Task<CommandRouterResult> RouteAsync(CommandRouterContext context)
        {
            // Stats
            RouteCalled = true;
            CommandString = context.CommandString;
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
                throw new ErrorException(explicitError);
            }

            return new CommandRouterResult();
        }

        public Task<TryResult<ICommandHandler>> TryFindHandlerAsync(CommandRouterContext context)
        {
            FindCalled = true;
            return Task.FromResult(new TryResult<ICommandHandler>(new MockCommandHandler()));
        }

        private readonly CancellationTokenSource? cancelOnRouteCalled;
        private readonly Exception? exception;
        private readonly Error? explicitError;
        private readonly int? routeDelay;
    }
}
