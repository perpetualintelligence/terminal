/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands;
using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Shared.Infrastructure;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockCommandRouter : ICommandRouter
    {
        public Task<CommandRouterResult> RouteAsync(CommandRouterContext context)
        {
            throw new System.NotImplementedException();
        }

        public Task<OneImlxTryResult<ICommandHandler>> TryFindHandlerAsync(CommandRouterContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
