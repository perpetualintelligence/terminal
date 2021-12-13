/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved
    https://perpetualintelligence.com
    https://api.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Routers;
using PerpetualIntelligence.Protocols.Abstractions;

namespace PerpetualIntelligence.Cli.Commands.Handlers
{
    /// <summary>
    /// An abstraction to handle a <c>cli</c> command request routed from a <see cref="ICommandRouter"/>.
    /// </summary>
    public interface ICommandHandler : IHandler<CommandHandlerContext, CommandHandlerResult>
    {
    }
}
