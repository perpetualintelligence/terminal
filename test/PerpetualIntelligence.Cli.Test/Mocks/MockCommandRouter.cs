/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Shared.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockCommandRouter : ICommandRouter
    {
        public MockCommandRouter(int? routeDelay = null, CancellationTokenSource? cancelOnRouteCalled = null, Exception? exception = null)
        {
            this.routeDelay = routeDelay;
            this.cancelOnRouteCalled = cancelOnRouteCalled;
            this.exception = exception;
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

            return new CommandRouterResult();
        }

        public Task<OneImlxTryResult<ICommandHandler>> TryFindHandlerAsync(CommandRouterContext context)
        {
            FindCalled = true;
            return Task.FromResult(new OneImlxTryResult<ICommandHandler>(new MockCommandHandler()));
        }

        private readonly Exception? exception;
        private CancellationTokenSource? cancelOnRouteCalled;
        private int? routeDelay;
    }
}
