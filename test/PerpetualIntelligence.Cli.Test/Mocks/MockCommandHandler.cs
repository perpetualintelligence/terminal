/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
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
