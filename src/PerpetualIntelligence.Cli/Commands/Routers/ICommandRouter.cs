/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com
*/

using PerpetualIntelligence.Cli.Commands.Handlers;
using PerpetualIntelligence.Protocols.Abstractions;

namespace PerpetualIntelligence.Cli.Commands.Routers
{
    /// <summary>
    /// An abstraction of a command router.
    /// </summary>
    public interface ICommandRouter : IRouter<CommandRouterContext, CommandRouterResult, ICommandHandler>
    {
    }
}
