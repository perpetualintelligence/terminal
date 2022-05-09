/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Shared.Infrastructure;
using System;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockCommandRouterCancellation : ICommandRouter
    {
        public Task<CommandRouterResult> RouteAsync(CommandRouterContext context)
        {
            return Task.FromResult(new CommandRouterResult());
        }

        public Task<TryResultOrError<ICommandHandler>> TryFindHandlerAsync(CommandRouterContext context)
        {
            throw new OperationCanceledException();
        }
    }
}
