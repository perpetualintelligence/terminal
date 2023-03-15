/*
    Copyright (c) Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using PerpetualIntelligence.Cli.Commands.Routers;
using System.Threading.Tasks;

namespace PerpetualIntelligence.Cli.Commands.Handlers
{
    /// <summary>
    /// An abstraction to handle a <c>pi-cli</c> command request routed from a <see cref="ICommandRouter"/>.
    /// </summary>
    public interface ICommandHandler
    {
        /// <summary>
        /// Handles the command request.
        /// </summary>
        /// <param name="context">The handler context.</param>
        /// <returns>The handler result.</returns>
        Task<CommandHandlerResult> HandleAsync(CommandHandlerContext context);
    }
}