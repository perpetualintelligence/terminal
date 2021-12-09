/*
    Copyright (c) 2019-2022. All Rights Reserved. Perpetual Intelligence L.L.C.
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Handlers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Mocks
{
    public class MockCommandHandler : ICommandHandler
    {
        public bool Called { get; set; }

        public Task<CommandHandlerResult> HandleAsync(CommandHandlerContext context)
        {
            Called = true;
            return Task.FromResult(new CommandHandlerResult());
        }
    }
}
